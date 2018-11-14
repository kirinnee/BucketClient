using Newtonsoft.Json;

namespace BucketClient.AWS
{
    internal class Policy
    {
        public string Id { get; private set; }
        public string Version { get; private set; }
        public PolicyStatement[] Statement { get; private set; }

        public Policy(string id, string version, PolicyStatement[] statement)
        {
            Id = id;
            Version = version;
            Statement = statement;
        }

    }

    internal class PolicyStatement
    {


        public string Sid { get; private set; }
        public string[] Action { get; private set; }
        public string Effect { get; private set; }
        public string Resource { get; private set; }
        
        public string Principal { get; private set; }
        public PolicyStatement(string sid, string[] action, string effect, string resource, string principal)
        {
            Sid = sid;
            Action = action;
            Effect = effect;
            Resource = resource;
            Principal = principal;
        }
    }
}
