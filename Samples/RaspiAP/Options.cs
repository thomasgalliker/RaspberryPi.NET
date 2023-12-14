using System.CommandLine;

namespace RaspiAP
{
    /// <summary>
    /// A set of <see cref="Option{T}"/> to be shared across different commands.
    /// </summary>
    public static class CommonOptions
    {
        public static readonly Option<string> CountryOption = new Option<string>(
            aliases: new[] { "--country" },
            description: "The country code")
        {
            IsRequired = false,
        };
    }
}