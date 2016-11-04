using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpdateLocalTimeNTP
{
    public class ServerListHandler
    {
        const string FULL_SERVER_LIST_FILE = "servers.txt";
        const string ACTIVE_SERVER_LIST_FILE = "active.txt";
        public const int MAX_SERVER_REQUEST = 30;
        public const int MIN_SERVER_REQUEST = 3;
        static public string[] StartingServers = new string[]
        {   "0.north-america.pool.ntp.org", "1.north-america.pool.ntp.org", "2.north-america.pool.ntp.org", "3.north-america.pool.ntp.org", 
            "augean.eleceng.adelaide.edu.au", "benoni.uit.no", "biofiz.mf.uni-lj.si", "bonehed.lcs.mit.edu", "canon.inria.fr", "canon.inria.fr", "chronos1.umt.edu", 
            "chronos2.umt.edu", "chronos3.umt.edu", "clepsydra.dec.com", "clock.cuhk.edu.hk", "clock.isc.org", "clock.linuxshell.net", "clock.nc.fukuoka-u.ac.jp", 
            "clock.netcetera.dk", "clock.psu.edu", "clock.tl.fukuoka-u.ac.jp", "clock.via.net", "clock2.netcetera.dk", "cronos.cenam.mx", "darkcity.cerias.purdue.edu", 
            "fartein.ifi.uio.no", "fuzz.psc.edu", "gilbreth.ecn.purdue.edu", "harbor.ecn.purdue.edu", "info.cyf-kr.edu.pl", "louie.udel.edu", "molecule.ecn.purdue.edu", 
            "navobs1.usnogps.navy.mil", "navobs1.wustl.edu", "navobs2.usnogps.navy.mil", "ncar.ucar.edu", "nic.near.net", "nist.expertsmi.com", "nist.netservicesgroup.com", 
            "nist.time.nosc.us", "nist1.aol-ca.truetime.com", "nist1.aol-va.symmetricom.com", "nist1.symmetricom.com", "nist1-atl.ustiming.org", "nist1-chi.ustiming.org", 
            "nist1-la.ustiming.org", "nist1-lnk.binary.net", "nist1-lv.ustiming.org", "nist1-macon.macon.ga.us", "nist1-nj.ustiming.org", "nist1-nj2.ustiming.org", 
            "nist1-ny.ustiming.org", "nist1-ny2.ustiming.org", "nist1-pa.ustiming.org", "nist1-sj.ustiming.org", "nisttime.carsoncity.k12.mi.us", "nist-time-server.eoni.com", 
            "noc.near.net", "ns.nts.umn.edu", "nss.nts.umn.edu", "ntp.adelaide.edu.au", "ntp.certum.pl", "ntp.cpsc.ucalgary.ca", "ntp.cs.mu.oz.au", "ntp.cs.strath.ac.uk", 
            "ntp.cs.tcd.ie", "ntp.cs.unp.ac.za", "ntp.ctr.columbia.edu", "ntp.dgf.uchile.cl", "ntp.massey.ac.nz", "ntp.maths.tcd.ie", "ntp.obspm.fr", "ntp.psn.ru", 
            "ntp.public.otago.ac.nz", "ntp.saard.net", "ntp.tcd.ie", "ntp.tmc.edu", "ntp.to.themasses.org", "ntp.ucsd.edu", "ntp.univ-lyon1.fr", "ntp.univ-lyon1.fr", 
            "ntp.waikato.ac.nz", "ntp0.cornell.edu", "ntp-0.cso.uiuc.edu", "ntp0.fau.de", "ntp0.nl.net", "ntp0.pipex.net", "ntp0.strath.ac.uk", "ntp1.cmc.ec.gc.ca", 
            "ntp1.cmc.ec.gc.ca", "ntp1.cs.wisc.edu", "ntp-1.cso.uiuc.edu", "ntp1.delmarva.com", "ntp-1.ece.cmu.edu", "ntp1.fau.de", "ntp-1.mcs.anl.gov", "ntp1.mpis.net", 
            "ntp1.nl.net", "ntp1.pipex.net", "ntp1.strath.ac.uk", "ntp-1.vt.edu", "ntp2.cs.wisc.edu", "ntp-2.cso.uiuc.edu", "ntp-2.ece.cmu.edu", "ntp2.fau.de", 
            "ntp-2.mcs.anl.gov", "ntp2.mpis.net", "ntp2.nl.net", "ntp2.pipex.net", "ntp2.strath.ac.uk", "ntp-2.vt.edu", "ntp2a.mcc.ac.uk", "ntp2b.mcc.ac.uk", "ntp2c.mcc.ac.uk", 
            "ntp2d.mcc.ac.uk", "ntp3.cs.wisc.edu", "ntp3.strath.ac.uk", "ntp3.tamu.edu", "ntp5.tamu.edu", "ntp-cup.external.hp.com", "ntp-nist.ldsbc.edu", "ntps1-0.cs.tu-berlin.de", 
            "ntps1-0.uni-erlangen.de", "ntps1-1.cs.tu-berlin.de", "ntps1-1.uni-erlangen.de", "ntps1-2.uni-erlangen.de", "ntp-sop.inria.fr", "os.ntp.carnet.hr", "otc1.psu.edu", 
            "ptbtime1.ptb.de", "ptbtime2.ptb.de", "ri.ntp.carnet.hr", "rolex.peachnet.edu", "slug.ctv.es", "st.ntp.carnet.hr", "sundial.columbia.edu", "swisstime.ethz.ch", 
            "tick.cs.unlv.edu", "tick.keso.fi", "tick.usno.navy.mil", "tick.utoronto.ca", "time.chu.nrc.ca", "time.deakin.edu.au", "time.ien.it", "time.ijs.si", "time.kfki.hu", 
            "time.nist.gov", "time.nrc.ca", "time.nuri.net", "time1.stupi.se", "time2.stupi.se", "time-a.nist.gov", "time-a.timefreq.bldrdoc.gov", "time-b.nist.gov", 
            "time-b.timefreq.bldrdoc.gov", "time-c.nist.gov", "time-c.timefreq.bldrdoc.gov", "time-d.nist.gov", "timekeeper.isi.edu", "timelord.uregina.ca", 
            "time-nist.symmetricom.com", "time-nw.nist.gov", "timex.cs.columbia.edu", "timex.peachnet.edu", "tk1.ihug.co.nz", "tk2.ihug.co.nz", "tk3.ihug.co.nz", 
            "tock.cs.unlv.edu", "tock.keso.fi", "tock.usno.navy.mil", "tock.utoronto.ca", "usno.pa-x.dec.com", "utcnist.colorado.edu", "utcnist2.colorado.edu", 
            "vega.cbk.poznan.pl", "vtserf.cc.vt.edu", "wolfnisttime.com", "wwv.nist.gov", "zg1.ntp.carnet.hr", "zg2.ntp.carnet.hr"};

        static private string[] GetFullList(bool alsoLog, string logFile)
        {
            string[] servers = null;
            try
            {
                servers = System.IO.File.ReadAllLines(FULL_SERVER_LIST_FILE);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Common.WriteAndLogThisLine(" *Problem getting '" + FULL_SERVER_LIST_FILE + "', Exception as follows:", alsoLog, true, logFile);
                Common.WriteAndLogThisLine(ex.Message, alsoLog, true, logFile);
                Common.WriteAndLogThisLine(" *Using internal default list.", alsoLog, true, logFile);
                try
                {
                    System.IO.File.WriteAllLines(FULL_SERVER_LIST_FILE, StartingServers);
                }
                catch { }
                return StartingServers;
            }
            return servers;
        }

        static private string[] GetActiveList(bool alsoLog, string logFile)
        {
            string[] servers = null;
            try
            {
                servers = System.IO.File.ReadAllLines(ACTIVE_SERVER_LIST_FILE);
            }
            catch //(Exception ex)
            {
                servers = GetFullList(alsoLog, logFile); //assume GetFullList ALWAYS returns something
                try
                {
                    System.IO.File.WriteAllLines(ACTIVE_SERVER_LIST_FILE, servers);
                }
                catch { }      
            }
            return servers;
        }

        static public bool ResetActiveServers(bool alsoLog, string logFile)
        {
            string[] servers = GetFullList(alsoLog, logFile);
            try
            {
                System.IO.File.WriteAllLines(ACTIVE_SERVER_LIST_FILE, servers);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Common.WriteAndLogThisLine(" *Problem writting '" + ACTIVE_SERVER_LIST_FILE + "', Exception as follows:", alsoLog, true, logFile);
                Common.WriteAndLogThisLine(ex.Message, alsoLog, true, logFile);
                return false;
            }
            return true;
        }

        static public bool UpdateActiveServers(List<string> serversToRemove, bool alsoLog, string logFile)
        {
            return UpdateActiveServers(serversToRemove.ToArray<string>(), alsoLog, logFile);
        }

        static public bool UpdateActiveServers(string[] serversToRemove, bool alsoLog, string logFile)
        {
            List<string>servers = GetActiveList(alsoLog, logFile).ToList<string>();
            if (servers != null)
            {
                foreach (string server in serversToRemove)
                {
                    if (servers.Contains(server))
                        servers.Remove(server);
                }
                try
                {
                    System.IO.File.WriteAllLines(ACTIVE_SERVER_LIST_FILE, servers);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Common.WriteAndLogThisLine(" *Problem writting '" + ACTIVE_SERVER_LIST_FILE + "', Exception as follows:", alsoLog, true, logFile);
                    Common.WriteAndLogThisLine(ex.Message, alsoLog, true, logFile);
                    return false;
                } 
            }
            return true;
        }

        static public string[] GetRandomActiveServers(int count, bool alsoLog, string logFile)
        {
            if (count < MIN_SERVER_REQUEST || count > MAX_SERVER_REQUEST)
                throw new ArgumentOutOfRangeException("count", "*Count must be between " + MIN_SERVER_REQUEST + " and " +
                    MAX_SERVER_REQUEST + " (inclusive).  Passed count=" + count);
            Random rnd = new Random(Environment.TickCount);
            string[] startingServers = GetActiveList(alsoLog, logFile);
            List<string> results = new List<string>(count);
            if (startingServers.Count() < count)
            {
                if (!ResetActiveServers(alsoLog, logFile))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Common.WriteAndLogThisLine(" *Problem getting sufficient active servers!", alsoLog, true, logFile);
                    return null;
                }
                else
                    startingServers = GetActiveList(alsoLog, logFile);
            }
            int LowBound = startingServers.GetLowerBound(0);
            int HighBound = startingServers.GetUpperBound(0);
            List<int> alreadyPicked = new List<int>();
            alreadyPicked.Add(-1); // prime the pump for the DO loop later
            for (int index = 0; index < count; index++)
            {
                int pickMe = -1;
                do { pickMe = rnd.Next(LowBound, HighBound); } while (alreadyPicked.Contains(pickMe));
                alreadyPicked.Add(pickMe);
                results.Add(startingServers[pickMe]);
            }
            return results.ToArray<string>();
        }

        //*/
    }
}
