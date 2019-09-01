using System.Net.NetworkInformation;

namespace Client.Infrastructure
{
    public static class CommonExtensions
    {
        public static int ToInt(this string str)
        {
            int num = -1;
            int.TryParse(str, out num);
            return num;
        }
    }
}