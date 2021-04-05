namespace URLRedirection
{
    public class RedirectionResult
    {
        /// <summary>
        /// If the Redirection was Found
        /// </summary>
        public bool RedirectionFound { get; set; }
        /// <summary>
        /// The Redirection Url
        /// </summary>
        public string RedirectUrl { get; set; }
        /// <summary>
        /// What type of redirection, either 301 or 302
        /// </summary>
        public int RedirectType { get; set; }
    }
}
