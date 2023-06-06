namespace DanekWPF
{
    public class Task
    {
        public string Name { get; set; }
        public Method Method { get; set; }
        public string Description { get; set; }
        public string Result { get; set; }
        public string Input1 { get; set; }
        public string Input2 { get; set; }
        public string Eps { get; set; }
        public string X0 { get; set; }
        public string MaxIt { get; set; }
        public string Omega { get; set; }

        public Task(
            string name,
            Method method,
            string description,
            string result,
            string input1,
            string input2,
            string eps,
            string x0,
            string maxIt,
            string omega = null)
        {
            Name = name;
            Method = method;
            Description = description;
            Result = result;
            Input1 = input1;
            Input2 = input2;
            Eps = eps;
            X0 = x0;
            MaxIt = maxIt;
            Omega = omega;
        }
    }
}
