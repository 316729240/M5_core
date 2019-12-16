namespace RazorEngine.Compilation
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using System;
    using System.CodeDom.Compiler;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web.Razor;
    using System.Web.Razor.Parser;
    using Templating;
    using RazorEngine.Core;
    using System.Runtime.InteropServices;
    /// <summary>
    /// Provides a base implementation of a direct compiler service.
    /// </summary>
    public abstract class DirectCompilerServiceBase : CompilerServiceBase, IDisposable
    {
        public static MetadataReference[] assemblieslist = null;
        #region Fields
        private readonly CodeDomProvider _codeDomProvider;
        private bool _disposed;
        #endregion
       /* string _CatchPath = "";
        public string CatchPath {
            set {
                _CatchPath = value;
            } 
            get {

                return _CatchPath ;
            }
        }*/
        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="DirectCompilerServiceBase"/>.
        /// </summary>
        /// <param name="codeLanguage">The razor code language.</param>
        /// <param name="codeDomProvider">The code dom provider used to generate code.</param>
        /// <param name="markupParserFactory">The markup parser factory.</param>
        protected DirectCompilerServiceBase(RazorCodeLanguage codeLanguage, CodeDomProvider codeDomProvider, Func<MarkupParser> markupParserFactory)
            : base(codeLanguage, markupParserFactory)
        {
            _codeDomProvider = codeDomProvider;
        }
        #endregion
        private byte[] CompileByte(string originalClassName, string originalText)
        {
            Console.WriteLine(assemblieslist);
            if (assemblieslist == null)
            {

                string replstr = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "file:///" : "file://";
                var assemblies = CompilerServicesUtility
                    .GetLoadedAssemblies()
                    .Where(a => !a.IsDynamic && File.Exists(a.CodeBase.Replace(replstr, "")))
                    .Select(a => (a.CodeBase.Replace(replstr, "")));

                int c = assemblies.Count();

                assemblieslist = new MetadataReference[c];

                int i = 0;
                foreach (string item in assemblies)
                {
                    assemblieslist[i] = (MetadataReference.CreateFromFile(item));
                    i++;
                }


            }
            CSharpCompilation compilation = null;
            var syntaxTree = CSharpSyntaxTree.ParseText(originalText);
            // 指定编译选项。
            var assemblyName = $"{originalClassName}.g";
            compilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree },
                   options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
               .AddReferences(
                   // 这算是偷懒了吗？我把 .NET Core 运行时用到的那些引用都加入到引用了。
                   // 加入引用是必要的，不然连 object 类型都是没有的，肯定编译不通过。
                   //AppDomain.CurrentDomain.GetAssemblies().Select(x => MetadataReference.CreateFromFile(x.Location))
                   assemblieslist
           );
            // 编译到内存流中。
            byte [] buff =null;
            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (result.Success)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    buff = ms.ToArray();
                }
                else
                {
                    string errmsg = "";
                    for (int i1 = 0; i1 < result.Diagnostics.Length; i1++)
                    {
                        errmsg += result.Diagnostics[i1] + "\r\n";
                    }
                    throw new Exception(errmsg);

                }
                ms.Close();
            }
            /*
            for (int i1=0;i1< list.Length;i1++)
            {
                list[i1] = null;
            }
            */
            return buff;

            // }
            //catch (Exception e)
            //{

            //}
            return null;
        }
        public override Type GetCompileType(TypeContext context, string name)
        {
            var key = Common.StrToMD5(name);
            string className = "C" + key;
            var assemblyPath = CatchPath + className + ".dll";
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            var compileUnit = GetCodeCompileUnit(context.ClassName, context.TemplateContent, context.Namespaces,
                                                 context.TemplateType, context.ModelType);

            var @params = new CompilerParameters
            {
                GenerateInMemory = true,
                OutputAssembly = @assemblyPath,
                GenerateExecutable = false,
                IncludeDebugInformation = false,
                CompilerOptions = "/target:library /optimize"
            };

            var assemblies = CompilerServicesUtility
                .GetLoadedAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a => a.Location);

            var includeAssemblies = (IncludeAssemblies() ?? Enumerable.Empty<string>());
            assemblies = assemblies.Concat(includeAssemblies)
                .Select(a => a.ToUpperInvariant())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Distinct();

            @params.ReferencedAssemblies.AddRange(assemblies.ToArray());

            string sourceCode = null;
            // if (Debug)
            //  {
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder, CultureInfo.InvariantCulture))
            {
                _codeDomProvider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions());
                sourceCode = builder.ToString();
            }
            byte [] buff=this.CompileByte(context.ClassName,sourceCode);
            System.IO.File.WriteAllBytes(assemblyPath,buff);
            var assembly = Assembly.Load(buff);
            return assembly.GetTypes().First(x => x.Name == context.ClassName);
            //  }
        }
        #region Methods
        /// <summary>
        /// Creates the compile results for the specified <see cref="TypeContext"/>.
        /// </summary>
        /// <param name="context">The type context.</param>
        /// <returns>The compiler results.</returns>
        [Pure]
        private Tuple<CompilerResults, string> Compile(TypeContext context,string name )
        {
            var key = Common.StrToMD5(name);
            string className = "C" + key;
            var assemblyPath =  CatchPath + className + ".dll";
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            var compileUnit = GetCodeCompileUnit(context.ClassName, context.TemplateContent, context.Namespaces,
                                                 context.TemplateType, context.ModelType);

            var @params = new CompilerParameters
            {
                GenerateInMemory = true,
                OutputAssembly = @assemblyPath,
                GenerateExecutable = false,
                IncludeDebugInformation = false,
                CompilerOptions = "/target:library /optimize"
            };

            var assemblies = CompilerServicesUtility
                .GetLoadedAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a => a.Location);

            var includeAssemblies = (IncludeAssemblies() ?? Enumerable.Empty<string>());
            assemblies = assemblies.Concat(includeAssemblies)
                .Select(a => a.ToUpperInvariant())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Distinct();

            @params.ReferencedAssemblies.AddRange(assemblies.ToArray());

            string sourceCode = null;
           // if (Debug)
          //  {
                var builder = new StringBuilder();
                using (var writer = new StringWriter(builder, CultureInfo.InvariantCulture))
                {
                    _codeDomProvider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions());
                    sourceCode = builder.ToString();
                }

              //  CompileType(context.ClassName, sourceCode);
          //  }
            CompilerResults r = _codeDomProvider.CompileAssemblyFromDom(@params, compileUnit);
            return Tuple.Create(r, sourceCode);
        }

        /// <summary>
        /// Compiles the type defined in the specified type context.
        /// </summary>
        /// <param name="context">The type context which defines the type to compile.</param>
        /// <returns>The compiled type.</returns>
        [Pure, SecurityCritical]
        public override Tuple<Type, Assembly> CompileType(TypeContext context )
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var result = Compile(context,"");
            var compileResult = result.Item1;

            if (compileResult.Errors != null && compileResult.Errors.Count > 0)
                throw new TemplateCompilationException(compileResult.Errors, result.Item2, context.TemplateContent);

            return Tuple.Create(
                compileResult.CompiledAssembly.GetType("CompiledRazorTemplates.Dynamic." + context.ClassName),
                compileResult.CompiledAssembly);
        }
        public override Tuple<Type, Assembly> CompileType2(TypeContext context, string name)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var result = Compile(context, name);
            var compileResult = result.Item1;
            if (compileResult.Errors != null && compileResult.Errors.Count > 0)
            {
                string msg = "";
                for(int i=0;i< compileResult.Errors.Count; i++)
                {
                    msg+=compileResult.Errors[i].ErrorText + "\r\n";
                }
                throw new Exception("模板编译错误：\r\n"+msg);
                //throw new TemplateCompilationException(compileResult.Errors, result.Item2, context.TemplateContent);
            }

            return Tuple.Create(
                compileResult.CompiledAssembly.GetType("CompiledRazorTemplates.Dynamic." + context.ClassName),
                compileResult.CompiledAssembly);
        }
        /// <summary>
        /// Releases managed resourced used by this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases managed resources used by this instance.
        /// </summary>
        /// <param name="disposing">Are we explicily disposing of this instance?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _codeDomProvider.Dispose();
                _disposed = true;
            }
        }
        #endregion
    }
}