using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Threading;
using Statistics;

namespace UpdateLocalTimeNTP
{
    class Program
    {
        static object LockObject = new object();
        static ConsoleColor StartingForeground = Console.ForegroundColor;
        static ConsoleColor StartingBackground = Console.BackgroundColor;

        static ConsoleColor ImportantColor = ConsoleColor.Cyan;
        static ConsoleColor WarningColor = ConsoleColor.Yellow;
        static ConsoleColor NormalColor = ConsoleColor.Gray;
        static ConsoleColor ErrorColor = ConsoleColor.Red;
        static ConsoleColor SuccessColor = ConsoleColor.DarkGreen;

        const string NoPauseSwitch = "-nopause";
        const string NoChangeSwitch = "-nochange";
        const string ForceSwitch = "-force";
        const string LogSwitch = "-log";
        const string FullServerListSwitch = "-resetlist";
        const string CountParameter = "-count:";
        const string MaxCorParameter = "-maxcor:";
        const string MinCorParameter = "-mincor:";
        private static readonly string[] HelpSwitches = { "-help", "-?", "/help", "/?", "?", "help" };
        static List<double> corrections = new List<double>();
        static int successes = 0;
        static List<string> failures = new List<string>();
        const int MinCorrection = 200; // bottom out at 200 milliseconds
        const int MaxCorrection = 90; // top out at 90 minutes
        static int MinUserCorrection = MinCorrection;
        static int MaxUserCorrection = MaxCorrection;
        static int DefaultServerCount = 15;
        static int serverCount = DefaultServerCount;

        static int Main(string[] args)
        {
            string logFile = DateTime.Now.ToString("yyyy-MM-dd HH") + ".log";
            bool alsoLog = args.Contains(LogSwitch);
            foreach (string s in HelpSwitches)
            {
                if (args.Contains(s))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\r\n===================================================================================================");
                    Console.WriteLine("UpdateLocalTimeNTP.exe Usage: *NOTE: PROGRAM MUST BE RUN AS ADMIN TO APPLY CHANGES!*");
                    Console.WriteLine("  -nochange  Test operation but make no clock changes.");
                    Console.WriteLine("  -force (cannot be used with -nochange) Force an update even if drastically off or tiny.");
                    Console.WriteLine("  -log  Log operations to local folder for unattended troubleshooting.");
                    Console.WriteLine("    Logs do not clean themselves up! Format=YEAR-MM-DD Hour.log (one log file per hour).");
                    Console.WriteLine("    Error logs are always written even if -log is not specified (ERROR-...log");
                    Console.WriteLine("  -nopause  Exit upon completion (allow console window to close).");
                    Console.WriteLine("  -resetlist  Re-add all servers, even ones that have failed in the past.");
                    Console.WriteLine("    If servers.txt is missing, program will recreate it from internal list.");
                    Console.WriteLine("    Edit active.txt or servers.txt if you want to use your own servers. (active.txt is rebuilt");
                    Console.WriteLine("    from servers.txt if present or internal list).");
                    Console.WriteLine("  -count:X (no spaces) Number of servers to use where X is between " +
                        ServerListHandler.MIN_SERVER_REQUEST + " and " + ServerListHandler.MAX_SERVER_REQUEST + " (inclusive, default " + DefaultServerCount + ")");
                    Console.WriteLine("  -mincor:X (no spaces) Number of minimum MILLISECONDS to allow for adjustment (default, " + MinCorrection + " ms)");
                    Console.WriteLine("  -maxcor:X (no spaces) Number of maximum MINUTES to allow for adjustment (default, " + MaxCorrection + " minutes)");
                    Console.WriteLine("    Corrections above the max or below the min will be ignored without -force switch.");
                    Console.WriteLine("=====================================================================================================\r\n");
                    return 0;
                }
            }
            if (args.Contains(FullServerListSwitch))
            {
                ServerListHandler.ResetActiveServers(alsoLog, logFile);
            }
            if (args.Contains(ForceSwitch) && args.Contains(NoChangeSwitch))
            {
                Console.ForegroundColor = WarningColor;
                Common.WriteAndLogThisLine("*INVALID SWITCH COMBINATION! Force and No Change cannot be used", true, true, logFile);
                Common.WriteAndLogThisLine("   at the same time!  Operation canceled*", true, true, logFile);
                return -1;
            }
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                Console.ForegroundColor = WarningColor;
                Common.WriteAndLogThisLine("*No Network Detected!  Check your network connections and be sure ", true, true, logFile);
                Common.WriteAndLogThisLine("  this machine is connected to the Internet.  Operation canceled*", true, true, logFile);
                return -1;
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ImportantColor;
            Common.WriteAndLogThisLine("Update Local Date/Time via NTP", alsoLog, false, logFile);
            Common.WriteAndLogThisLine("======================================================================", alsoLog, false, logFile);
            
            foreach(string a in args)
            {
                if (a.StartsWith(CountParameter) && a.Length > CountParameter.Length)
                {
                    int index = a.IndexOf(':');
                    if (index > 0)
                    {
                        string c = a.Substring(a.IndexOf(':') + 1);
                        if (!int.TryParse(c, out serverCount))
                        {
                            Console.ForegroundColor = WarningColor;
                            Common.WriteAndLogThisLine("*Warning (non-fatal): '-count:' flag value not an integer", true, false, logFile);
                            Common.WriteAndLogThisLine("   Defaulting to " + DefaultServerCount + " servers.", true, false, logFile);
                            serverCount = DefaultServerCount;
                        }
                        else if (serverCount > ServerListHandler.MAX_SERVER_REQUEST || serverCount < ServerListHandler.MIN_SERVER_REQUEST)
                        {
                            Console.ForegroundColor = WarningColor;
                            Common.WriteAndLogThisLine("*Warning (non-fatal): '-count:' flag value out of range " +
                                "(" + ServerListHandler.MIN_SERVER_REQUEST + " to " + ServerListHandler.MAX_SERVER_REQUEST + " inclusive)! ", true, false, logFile);
                            Common.WriteAndLogThisLine("   Defaulting to " + DefaultServerCount + " servers.", true, false, logFile);
                            serverCount = DefaultServerCount;
                        }
                    }
                }
                if (a.StartsWith(MaxCorParameter) && a.Length > MaxCorParameter.Length)
                {
                    int index = a.IndexOf(':');
                    if (index > 0)
                    {
                        string c = a.Substring(a.IndexOf(':') + 1);
                        if (!int.TryParse(c, out MaxUserCorrection))
                        {
                            Console.ForegroundColor = WarningColor;
                            Common.WriteAndLogThisLine("*Warning (non-fatal): '-maxcor:' flag value not an integer", true, false, logFile);
                            Common.WriteAndLogThisLine("   Defaulting to " + MaxCorrection + " minutes.", true, false, logFile);
                            MaxUserCorrection = MaxCorrection;
                        }
                        else if (MaxUserCorrection > 24*60 || MaxUserCorrection < 1)
                        {
                            Console.ForegroundColor = WarningColor;
                            Common.WriteAndLogThisLine("*Warning (non-fatal): '-maxcor:' flag value out of range " +
                                "(1 to " + (24 * 60).ToString() + " (=1 day) minutes inclusive)! ", true, false, logFile);
                            Common.WriteAndLogThisLine("   Defaulting to " + MaxCorrection + " minutes.", true, false, logFile);
                            MaxUserCorrection = MaxCorrection;
                        }
                    }
                }
                if (a.StartsWith(MinCorParameter) && a.Length > MinCorParameter.Length)
                {
                    int index = a.IndexOf(':');
                    if (index > 0)
                    {
                        string c = a.Substring(a.IndexOf(':') + 1);
                        if (!int.TryParse(c, out MinUserCorrection))
                        {
                            Console.ForegroundColor = WarningColor;
                            Common.WriteAndLogThisLine("*Warning (non-fatal): '-mincor:' flag value not an integer", true, false, logFile);
                            Common.WriteAndLogThisLine("   Defaulting to " + MinCorrection + " milliseconds.", true, false, logFile);
                            MinUserCorrection = MinCorrection;
                        }
                        else if (MinUserCorrection < 100 || MinUserCorrection > 600000)
                        {
                            Console.ForegroundColor = WarningColor;
                            Common.WriteAndLogThisLine("*Warning (non-fatal): '-mincor:' flag value out of range " +
                                "(100 to 600000 (=10 minutes) milliseconds inclusive)! ", true, false, logFile);
                            Common.WriteAndLogThisLine("   Defaulting to " + MinCorrection + " milliseconds.", true, false, logFile);
                            MinUserCorrection = MinCorrection;
                        }
                    }
                }
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = NormalColor;

            System.Diagnostics.Stopwatch programElapsedTime = new System.Diagnostics.Stopwatch();
            programElapsedTime.Start();
            
            Common.WriteAndLogThisLine(" Started: " + DateTimeOffset.Now.ToString(), alsoLog, false, logFile);
            Common.WriteAndLogThisLine(" Time corrections shown are additive differences between local and", alsoLog, false, logFile);
            Common.WriteAndLogThisLine(" remote (in seconds).", alsoLog, false, logFile);
            Common.WriteAndLogThisLine(" Negative numbers indicate local time is ahead of remote time.", alsoLog, false, logFile);

            if (args.Contains(NoChangeSwitch))
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ImportantColor;
                Common.WriteAndLogThisLine(" NO CHANGE switch active:  No actual changes will be applied to the system.", alsoLog, false, logFile);
                Console.ForegroundColor = NormalColor;
            }

            //===========DO THE WORK
            Console.ForegroundColor = NormalColor;
            string[] servers = ServerListHandler.GetRandomActiveServers(serverCount, alsoLog, logFile);
            Common.WriteAndLogThisLine("  Using random " + serverCount + " servers from active.txt...", alsoLog, false, logFile);
            //try
            //{
            //    servers = System.IO.File.ReadAllLines("servers.txt");
            //}
            //catch (Exception ex)
            if (servers == null || servers.Length < 3)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Common.WriteAndLogThisLine(" *Problem getting servers (null or too few), Operation Terminated!", alsoLog, true, logFile);
                Common.WriteAndLogThisLine("   Use '-resetlist' or delete 'active.txt' and 'servers.txt' to correct.", alsoLog, true, logFile);
                if (!args.Contains(NoPauseSwitch))
                {
                    Console.WriteLine("...press any key to terminate program...");
                    Console.ReadKey(true);
                }
                return -1;
            }
            if (serverCount > servers.Length)
            {
                Console.ForegroundColor = WarningColor;
                Common.WriteAndLogThisLine("*Warning (non-fatal): Requested server count not available, use '-resetlist' and try again.", alsoLog, false, logFile);
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ImportantColor;
            Common.WriteAndLogThisLine("=Begin server communication============", alsoLog, false, logFile);
            
            foreach (string server in servers)
            {
                if (string.IsNullOrWhiteSpace(server)) continue;
                
                NTPInformation stuff = new NTPInformation();
                NTP.InterrogateRemote(server, ref stuff);
                if (stuff.Success)
                {
                    successes++;
                    Console.ForegroundColor = SuccessColor;
                    Common.WriteAndLogThisLine("-" + server + " success, offset=" + stuff.Correction.TotalSeconds.ToString(), alsoLog, false, logFile);
                    corrections.Add(stuff.Correction.TotalSeconds);
                }
                else
                {
                    failures.Add(server);
                    Console.ForegroundColor = WarningColor;
                    Common.WriteAndLogThisLine("*" + server + " fail! ", alsoLog, false, logFile);// + stuff.Error.Message);
                }
                //*/
            }
            int returnVal = 0;
            Console.ForegroundColor = ImportantColor;
            Console.BackgroundColor = ConsoleColor.Black;
            Common.WriteAndLogThisLine("=End server communication==============", alsoLog, false, logFile);
            Console.ForegroundColor = NormalColor;
            Common.WriteAndLogThisLine(" Calculating...", alsoLog, false, logFile);
            if (corrections != null && corrections.Count >= 3)
            {
                
                Statistics.Boundaries b = Statistics.Outliers.GetOuterBoundaries(corrections, true);
                Common.WriteAndLogThisLine(" Upper Outlier Limit:" + b.High.ToString(), alsoLog, false, logFile);
                Common.WriteAndLogThisLine(" Lower Outlier Limit:" + b.Low.ToString(), alsoLog, false, logFile);
                double average = double.NaN;
                int datapoints = 0;
                foreach (double correction in corrections)
                {
                    if (!double.IsNaN(correction))
                    {
                        if (correction >= b.Low && correction <= b.High)
                        {
                            datapoints++;
                            if (double.IsNaN(average))
                                average = correction;
                            else
                                average += correction;
                        }
                        else
                        {
                            Console.ForegroundColor = WarningColor;
                            Console.BackgroundColor = ConsoleColor.Black;
                            Common.WriteAndLogThisLine("*Outlier Ignored=" + correction.ToString(), alsoLog, false, logFile);
                        }
                    }
                }
                
                if (datapoints > 0 && !double.IsNaN(average))
                {
                    average = Math.Round(average / datapoints, 7);
                    Console.ForegroundColor = NormalColor;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine(" Average difference : " + average.ToString() + " seconds.");
                    if (Math.Abs(average) < ((float)MinUserCorrection / 1000) && !args.Contains(ForceSwitch))
                    {
                        Console.ForegroundColor = ImportantColor;
                        Common.WriteAndLogThisLine(" Difference too small.  Time drift considered OK.", alsoLog, false, logFile);
                    }
                    else if (Math.Abs(average) > (MaxCorrection * 60) && !args.Contains(ForceSwitch))
                    {
                        Console.ForegroundColor = WarningColor;
                        Common.WriteAndLogThisLine(" *Warning: Difference TOO BIG! Time drift needs manual intervention, or use", alsoLog, true, logFile);
                        Common.WriteAndLogThisLine("  '-force' command-line switch.", alsoLog, true, logFile);
                        returnVal = -1;
                    }
                    else if (!args.Contains(NoChangeSwitch))
                    {
                        // maybe there should be a prompt here to ask the user if they didn't specify -nochange or -force???
                        if(!NTP.AdjustMachineTime(average))
                        {
                            Console.ForegroundColor = ErrorColor;
                            Common.WriteAndLogThisLine(" *FAILED TO UPDATE SYSTEM TIME!", alsoLog, true, logFile);
                            Common.WriteAndLogThisLine(" *System API call failed!", alsoLog, true, logFile);
                            returnVal = -1;
                        }
                        else
                            ServerListHandler.UpdateActiveServers(failures, alsoLog, logFile);  // remove failed servers from active list
                    }
                    else if (args.Contains(NoChangeSwitch))
                    {
                        Console.ForegroundColor = ImportantColor;
                        Common.WriteAndLogThisLine(" NO CHANGE switch active:  No actual changes will be applied to the system.", alsoLog, false, logFile);
                        ServerListHandler.UpdateActiveServers(failures, alsoLog, logFile);
                    }
                }
                else
                {
                    Console.ForegroundColor = WarningColor;
                    Common.WriteAndLogThisLine(" *Some strangeness in the data (internal error). No changes made.", alsoLog, true, logFile);
                    returnVal = -1;
                }
            }
            else if (corrections != null && corrections.Count < 3)
            {
                Console.ForegroundColor = WarningColor;
                Common.WriteAndLogThisLine(" *Not enough data to use! No changes made.", alsoLog, true, logFile);
                returnVal = -1;
            }
            else
            {
                Console.ForegroundColor = ErrorColor;
                Common.WriteAndLogThisLine(" *No data to use! No changes made.", alsoLog, true, logFile);
                returnVal = -1;
            }

            // all done, statistics then exit
            Console.ForegroundColor = NormalColor;
            Common.WriteAndLogThisLine(" Connections: " + successes + "  Failures: " + failures.Count(), alsoLog, false, logFile);
            Common.WriteAndLogThisLine(" Finished at: " + DateTimeOffset.Now.ToString(), alsoLog, false, logFile);
            Common.WriteAndLogThisLine(" Process Duration (hh:mm:ss.fffffff):  " + programElapsedTime.Elapsed.ToString(), alsoLog, false, logFile);
            Console.ForegroundColor = ImportantColor;
            Common.WriteAndLogThisLine("======================================================================", alsoLog, false, logFile);
            Console.BackgroundColor = StartingBackground;
            Console.ForegroundColor = StartingForeground;
            if (!args.Contains(NoPauseSwitch))
            {
                Common.WriteAndLogThisLine("...press any key to terminate program...", false, false, null);
                if (Console.KeyAvailable)
                    do { Console.ReadKey(true); } while (Console.KeyAvailable);
                Console.ReadKey(true);
            }
            return returnVal;
        }

    }
}
