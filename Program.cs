using System;
using System.Diagnostics;
using System.IO;
using Dota2GSI;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Threading;
using System.IO.Ports;
using Dota2GSI.Nodes;

namespace dotaLED
{
    static class Program
    {
        static GameStateListener gsl;
        static SerialPort serialPort;

        static void Main(string[] args)
        {

            serialPort = new SerialPort();
            serialPort.PortName = "com3";
            serialPort.BaudRate = 9600;
            serialPort.ReadTimeout = 500;
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
            serialPort.Open();

            Process[] process = Process.GetProcessesByName("Dota2");
            if(process.Length == 0)
            {
                Console.WriteLine("Dota2 is not running");
                Console.ReadLine();
                Environment.Exit(0);
            }
            gsl = new GameStateListener(4000);
            gsl.NewGameState += new NewGameStateHandler(OnNewGameState);

            if (!gsl.Start())
            {
                Console.WriteLine("GameStateListener could not start. Try running this program as Administrator.\r\nExiting.");
                Environment.Exit(0);
            }
            Console.WriteLine("Listening for game integration calls...");
        }

        static void OnNewGameState(GameState gs)
        {
            if (gs.Map.GameState == DOTA_GameState.DOTA_GAMERULES_STATE_GAME_IN_PROGRESS)
            {
               if(gs.Previously.Hero.Health != gs.Hero.Health)
                {
                    double hpPercent = gs.Hero.Health / gs.Hero.MaxHealth;
                    Console.WriteLine("HP percent" + gs.Hero.HealthPercent.ToString());
                    serialPort.WriteLine(gs.Hero.HealthPercent.ToString());
                } 
            }
        }
    }
}
