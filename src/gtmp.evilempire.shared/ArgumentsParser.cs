using System;
using System.Collections.Generic;

namespace gtmp.evilempire
{
    public class ArgumentsParser
    {
        static readonly string[] DefaultSwitchPrefixes = new[] { "--" };
        static readonly string DefaultSwitchValueDelimeter = ":";

        public delegate void Handler(object context, string value);

        readonly IDictionary<string, Handler> handlers = new Dictionary<string, Handler>();

        public IEnumerable<string> AllowedSwitchPrefixes { get; set; } = DefaultSwitchPrefixes;
        public string SwitchValueDelimeter { get; set; } = DefaultSwitchValueDelimeter;

        public void Parse(object context, string[] args)
        {
            if (args == null)
            {
                return;
            }

            foreach(var arg in args)
            {
                if (arg == null)
                {
                    continue;
                }

                var matchingPrefix = GetMatchingPrefix(arg, AllowedSwitchPrefixes);
                if (!string.IsNullOrEmpty(matchingPrefix))
                {
                    string key = arg.Substring(matchingPrefix.Length);
                    string value = null;
                    var delimeterIndex = key.IndexOf(SwitchValueDelimeter, StringComparison.OrdinalIgnoreCase);
                    if (delimeterIndex >= 0)
                    {
                        value = key.Substring(delimeterIndex + SwitchValueDelimeter.Length);
                        key = key.Substring(0, delimeterIndex);
                    }

                    Handler handler;
                    if (handlers.TryGetValue(key, out handler))
                    {
                        handler?.Invoke(context, value);
                    }
                }
            }
        }

        public ArgumentsParser AddHandler(string switchName, Handler handler)
        {
            handlers[switchName] = handler;
            return this;
        }

        static string GetMatchingPrefix(string text, IEnumerable<string> prefixes)
        {
            if (text == null)
            {
                return null;
            }
            foreach(var prefix in prefixes)
            {
                if (prefix == null)
                {
                    continue;
                }
                if (text.IndexOf(prefix, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return prefix;
                }
            }
            return null;
        }
    }

    public class ArgumentParser<T> : ArgumentsParser
    {
        public delegate void HandlerT(T context, string value);

        public void Parse(T context, string[] args)
        {
            base.Parse(context, args);
        }

        public ArgumentParser<T> AddHandler(string switchName, HandlerT handler)
        {
            base.AddHandler(switchName, (context, value) =>
            {
                T contextOfTargetType = (T)context;
                handler?.Invoke(contextOfTargetType, value);
            });
            return this;
        }
    }
}
