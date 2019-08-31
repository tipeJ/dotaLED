using System;
using System.Diagnostics;
using System.IO;
using Dota2GSI;
using Microsoft.Win32;
using System.Threading;
using System.IO.Ports;

namespace Dota2GSI_Example_program
{
    class Program
    {
        static SerialPort SP;
        static GameStateListener _gsl;

        static void Main(string[] args)
        {
            SP = new SerialPort();
            SP.PortName = "com3";
            SP.BaudRate = 9600;
            SP.ReadTimeout = 500;
            SP.Open();
            if (args == null) Console.WriteLine();

            CreateGsifile();

            Process[] pname = Process.GetProcessesByName("Dota2");
            if (pname.Length == 0)
            {
                Console.WriteLine("Dota 2 is not running. Please start Dota 2.");
                Console.ReadLine();
                Environment.Exit(0);
            }

            _gsl = new GameStateListener(4000);
            _gsl.NewGameState += OnNewGameState;


            if (!_gsl.Start())
            {
                Console.WriteLine("GameStateListener could not start. Try running this program as Administrator. Exiting.");
                Console.ReadLine();
                Environment.Exit(0);
            }
            Console.WriteLine("Listening for game integration calls...");

            Console.WriteLine("Press ESC to quit");
            do
            {
                while (!Console.KeyAvailable)
                {
                    Thread.Sleep(1000);
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
        static void OnNewGameState(GameState gs)
        {
            Console.Clear();
            Console.WriteLine("Press ESC to quit");
            Console.WriteLine("Current Dota version: " + gs.Provider.Version);
            Console.WriteLine("Current time as displayed by the clock (in seconds): " + gs.Map.ClockTime);
            Console.WriteLine("Your steam name: " + gs.Player.Name);
            Console.WriteLine("hero ID: " + gs.Hero.ID);
            Console.WriteLine("Your HP: " + gs.Hero.Health);

            if (gs.Previously.Hero.Health != gs.Hero.Health)
            {
                double hpPercent = gs.Hero.Health / gs.Hero.MaxHealth;
                Console.WriteLine("HP percent" + gs.Hero.HealthPercent.ToString());
                SP.WriteLine("HP:" + gs.Hero.HealthPercent.ToString());
            }
            Console.WriteLine("Current Hero Name:" + gs.Hero.Name);
            SP.WriteLine("HERO:" + gs.Hero.Name);
            if (gs.Hero.IsMagicImmune != gs.Previously.Hero.IsMagicImmune)
            {
                //If the BKB status is different from the previous gamestate, write the updated status to the serialport in the form of String boolean (true / false), which will be parsed in the arduino script
                SP.WriteLine("BKB:" + gs.Hero.IsMagicImmune.ToString().ToLower());
            }
        }

        private static void CreateGsifile()
        {
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");

            if (regKey != null)
            {
                string gsifolder = regKey.GetValue("SteamPath") +
                                   @"\steamapps\common\dota 2 beta\game\dota\cfg\gamestate_integration";
                Directory.CreateDirectory(gsifolder);
                string gsifile = gsifolder + @"\gamestate_integration_testGSI.cfg";
                if (File.Exists(gsifile))
                    return;

                string[] contentofgsifile =
                {
                    "\"Dota 2 Integration Configuration\"",
                    "{",
                    "    \"uri\"           \"http://localhost:4000\"",
                    "    \"timeout\"       \"5.0\"",
                    "    \"buffer\"        \"0.1\"",
                    "    \"throttle\"      \"0.1\"",
                    "    \"heartbeat\"     \"30.0\"",
                    "    \"data\"",
                    "    {",
                    "        \"provider\"      \"1\"",
                    "        \"map\"           \"1\"",
                    "        \"player\"        \"1\"",
                    "        \"hero\"          \"1\"",
                    "        \"abilities\"     \"1\"",
                    "        \"items\"         \"1\"",
                    "    }",
                    "}",

                };

                File.WriteAllLines(gsifile, contentofgsifile);
            }
            else
            {
                Console.WriteLine("Steam registry key not found, cannot create a GSI file");
                Console.ReadLine();
                Environment.Exit(0);
            }
        }
    }
}