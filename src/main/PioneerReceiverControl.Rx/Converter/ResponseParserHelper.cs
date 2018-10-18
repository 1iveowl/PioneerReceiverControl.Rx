using System.Linq;
using IPioneerReceiverControl.Rx.Model;
using IPioneerReceiverControl.Rx.Model.Command;

namespace PioneerReceiverControl.Rx.Converter
{
    internal static class ResponseParserHelper
    {
        private const string Wildcard = "*";

        internal static string MatchResponse(IReceiverCommandDefinition commandDefinition, IRawResponseData response)
        {
            var responseWithOutWildcard = SplitNameFromParameter(commandDefinition.ResponseTemplate, response.Data);

            return responseWithOutWildcard.response == responseWithOutWildcard.template
                ? responseWithOutWildcard.parameter
                : null;
        }

        private static (string template, string response, string parameter) SplitNameFromParameter(string template,
            string response)
        {
            if (response.Length != template.Length)
            {
                return (null, null, null);
            }

            var templateWithoutWildcards = template.Replace(Wildcard, string.Empty);

            var lenTemplateWithoutWildcards = templateWithoutWildcards.Length;

            var responseName = string.Join("", response.Take(lenTemplateWithoutWildcards));

            var parameter = response.Replace(templateWithoutWildcards, string.Empty);

            return (templateWithoutWildcards, responseName, parameter);
        }
    }
}
