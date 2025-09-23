namespace FNAExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var game = new NanoUIGame())
            {
                game.Run();
            }
        }
    }
}
