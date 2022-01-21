using System;
using System.Linq;
using static BBCoders.Commons.Utilities.AnsiConstants;

namespace  BBCoders.Commons.Utilities
{
    /// <summary>
    /// Reporter
    /// </summary>
    public static class Reporter
    {
        /// <summary>
        /// verbose logs
        /// </summary>
        /// <value></value>
        public static bool IsVerbose { get; set; }
        /// <summary>
        /// no color for logs
        /// </summary>
        /// <value></value>
        public static bool NoColor { get; set; }
        /// <summary>
        /// prefix output of the logs
        /// </summary>
        /// <value></value>
        public static bool PrefixOutput { get; set; }

        /// <summary>
        /// color the logs
        /// </summary>
        /// <param name="value"></param>
        /// <param name="colorizeFunc"></param>
        /// <returns></returns>
        public static string Colorize(string value, Func<string, string> colorizeFunc)
            => NoColor ? value : colorizeFunc(value);

        /// <summary>
        /// write error logs
        /// </summary>
        /// <param name="message"></param>
        public static void WriteError(string message)
            => WriteLine(Prefix("error:   ", Colorize(message, x => Bold + Red + x + Reset)));

        /// <summary>
        /// write warning logs
        /// </summary>
        /// <param name="message"></param>
        public static void WriteWarning(string message)
            => WriteLine(Prefix("warn:    ", Colorize(message, x => Bold + Yellow + x + Reset)));

        /// <summary>
        /// write info logs
        /// </summary>
        /// <param name="message"></param>
        public static void WriteInformation(string message)
            => WriteLine(Prefix("info:    ", message));

        /// <summary>
        /// write log
        /// </summary>
        /// <param name="message"></param>
        public static void WriteData(string message)
            => WriteLine(Prefix("data:    ", Colorize(message, x => Bold + Gray + x + Reset)));

        /// <summary>
        /// write verbose logs
        /// </summary>
        /// <param name="message"></param>
        public static void WriteVerbose(string message)
        {
            if (IsVerbose)
            {
                WriteLine(Prefix("verbose: ", Colorize(message, x => Bold + Magenta + x + Reset)));
            }
        }

        private static string Prefix(string prefix, string value)
            => PrefixOutput
                ? value == null
                    ? prefix
                    : string.Join(
                        Environment.NewLine,
                        value.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Select(l => prefix + l))
                : value;

        private static void WriteLine(string value)
        {
            if (NoColor)
            {
                Console.WriteLine(value);
            }
            else
            {
                Console.Out.WriteLine(value);
            }
        }
    }
}