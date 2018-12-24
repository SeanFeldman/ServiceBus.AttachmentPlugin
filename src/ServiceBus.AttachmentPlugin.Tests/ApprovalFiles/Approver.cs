// Adopted from https://github.com/Particular/Particular.Approvals/blob/master/src/Particular.Approvals/Approver.cs

namespace Xunit
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Verifies that values contain approved content.
    /// </summary>
    static class Approver
    {
        static string TestDirectory
        {
            get
            {
                var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
                var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
                var dirPath = Path.GetDirectoryName(codeBasePath);
                return Path.Combine(dirPath, "..", "..", "..", "ApprovalFiles");
            }
        }

        static readonly string approvalFilesPath = TestDirectory;
        static readonly JsonSerializerSettings jsonSerializerSettings;

        static Approver()
        {
            jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
        }

        /// <summary>
        /// Verifies that the received string matches the contents of the corresponding approval file.
        /// </summary>
        /// <param name="value">The string to verify.</param>
        /// <param name="scrubber">A delegate that modifies the received string before comparing it to the approval file.</param>
        /// <param name="scenario">A value that will be added to the name of the approval file.</param>
        public static void Verify(string value, Func<string, string> scrubber = null, string scenario = null)
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1);
            var currentMethodName = sf.GetMethod();

            var className = currentMethodName.DeclaringType.Name;
            var methodName = currentMethodName.Name;
            var scenarioName = string.IsNullOrEmpty(scenario) ? "" : scenario + ".";

            if (scrubber != null)
            {
                value = scrubber(value);
            }

            var receivedFile = Path.Combine(approvalFilesPath, $"{className}.{methodName}.{scenarioName}received.txt");
            File.WriteAllText(receivedFile, value);

            var approvedFile = Path.Combine(approvalFilesPath, $"{className}.{methodName}.{scenarioName}approved.txt");
            var approvedText = File.ReadAllText(approvedFile);

            var normalizedApprovedText = approvedText.Replace("\r\n", "\n");
            var normalizedReceivedText = value.Replace("\r\n", "\n");

            Assert.Equal(normalizedApprovedText, normalizedReceivedText);

            File.Delete(receivedFile);
        }

        /// <summary>
        /// Verifies that the received object, after it has been serialized, matches the contents of the corresponding approval file.
        /// </summary>
        /// <param name="value">The object to verify.</param>
        /// <param name="scrubber">A delegate that modifies the received object, after it has been serialized, before comparing it to the approval file.</param>
        /// <param name="scenario">A value that will be added to the name of the approval file.</param>
        public static void Verify(object value, Func<string, string> scrubber = null, string scenario = null)
        {
            var json = JsonConvert.SerializeObject(value, jsonSerializerSettings);

            Verify(json, scrubber, scenario);
        }
    }
}