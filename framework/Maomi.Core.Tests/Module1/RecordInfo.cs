namespace Maomi.Core.Tests.Module1
{
    public class RecordInfo
    {
        private readonly List<string> _list = new List<string>();
        public IReadOnlyList<string> List => _list;
        public void Add(string name)
        {
            _list.Add(name);
        }
    }
}
