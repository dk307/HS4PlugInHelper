using System;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Hspi.Utils
{
    internal static class ExceptionHelper
    {
        public static string GetFullMessage(this Exception ex, bool debugMode = false)
        {
            var stb = new StringBuilder();
            switch (ex)
            {
                case AggregateException aggregationException:
                    foreach (var innerException in aggregationException.InnerExceptions)
                    {
                        stb.AppendLine(GetFullMessage(innerException, debugMode));
                    }
                    break;

                default:
                    {
                        stb.AppendLine(debugMode ? ex.ToString() : ex.Message);
                        if (ex.InnerException != null)
                        {
                            stb.AppendLine(GetFullMessage(ex.InnerException, debugMode));
                        }
                    }
                    break;
            }

            return stb.ToString();
        }

        public static bool IsCancelException(this Exception ex)
        {
            return (ex is TaskCanceledException) ||
                   (ex is OperationCanceledException) ||
                   (ex is ObjectDisposedException);
        }
    };
}