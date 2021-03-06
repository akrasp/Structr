using Structr.IO;
using System;
using System.Threading.Tasks;

namespace Structr.Samples.IO
{
    public class App : IApp
    {
        private readonly IStringWriter _writer;

        public App(IStringWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            _writer = writer;
        }

        public async Task RunAsync()
        {
            PathHelper.Configure(options =>
            {
                options.Directories[EDirectory.Base] = @"C:\Creacode";
                options.Directories[EDirectory.Data] = @"C:\Creacode\App_Data";                
            });

            await _writer.WriteLineAsync(PathHelper.Combine(EDirectory.Base, "foo.bar"));
            await _writer.WriteLineAsync(PathHelper.Format(@"|BaseDirectory|\foo.bar"));
            await _writer.WriteLineAsync(PathHelper.Format(@"|DataDirectory|\foo\bar\baz"));
        }
    }
}
