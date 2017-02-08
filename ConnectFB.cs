using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Xml;
using System.IO;
using System.Threading;

namespace ConnectFB
{
    class Program
    {
        public static void FBODBC()
        {
            Console.Write("inside FBODBC");
            String key = "";

            // XML login string
            String loginCommand = createLoginXml("admin", "admin");
            Console.Write("Client Started... \n");
            ConnectionObject connectionObject = new ConnectionObject();

            //Send Login Command once to get fishbowl server to recognize the connection attempt
            //or pull the key off the line if already connected
            key = pullKey(connectionObject.sendCommand(loginCommand));
            if (key == "null")
            {
                Console.Write("Please accept the connection attempt on the fisbowl server and press return");
                Console.ReadLine();
                key = pullKey(connectionObject.sendCommand(loginCommand));
            }

            //Now create the query to pull data out of fishbowl using the key
            //String customerNameList = connectionObject.sendCommand(listCustomerName(key));
            //Console.Write(customerNameList);

            //this call work fine
            String partList = connectionObject.sendCommand(listPartList(key));
            Console.Write(partList);

            //this ExecuteQueryRq doesn't work: throws "This stream does not support seek operations"
            //String inventoryList = connectionObject.sendCommand(listInventory(key));
            String inventoryList = connectionObject.sendCommand("<ExecuteQueryRq><Query>SELECT * from company</Query></ExecuteQueryRq>");
            Console.Write(inventoryList);

            //Added to keep the cmd screen open
            Console.ReadLine();

        }

        private static String createLoginXml(string username, string password)
        {
            System.Text.StringBuilder buffer = new System.Text.StringBuilder("<FbiXml><Ticket/><FbiMsgsRq><LoginRq><IAID>");
            buffer.Append("222");
            buffer.Append("</IAID><IAName>");
            buffer.Append("Inventory Reconcile");
            buffer.Append("</IAName><IADescription>");
            buffer.Append("Fishbowl/Quickbooks Inventory Reconcile");
            buffer.Append("</IADescription><UserName>");
            buffer.Append(username);
            buffer.Append("</UserName><UserPassword>");

            MD5 md5 = MD5CryptoServiceProvider.Create();
            byte[] encoded = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(password));
            string encrypted = Convert.ToBase64String(encoded, 0, 16);
            buffer.Append(encrypted);
            buffer.Append("</UserPassword></LoginRq></FbiMsgsRq></FbiXml>");

            return buffer.ToString();
        }

        //Pull the session Key out of the server response string
        private static String pullKey(String connection)
        {
            String key = "";
            using (XmlReader reader = XmlReader.Create(new StringReader(connection)))
            {
                while (reader.Read())
                {
                    //if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("Key"))
                    if (reader.Name.Equals("Key") && reader.Read())
                    {
                        return reader.Value.ToString();
                    }
                }
            }
            return key;
        }


        // The following generates different querries 
        private static string listCustomerName(string key)
        {
            return "<FbiXml><Ticket><Key>" + key + "</Key></Ticket><FbiMsgsRq><CustomerNameListRq></CustomerNameListRq></FbiMsgsRq></FbiXml>";
        }

        private static string listPartList(string key)
        {
            return "<FbiXml><Ticket><Key>" + key + "</Key></Ticket><FbiMsgsRq><InvQtyRq><PartNum>B202</PartNum><LastModifiedFrom></LastModifiedFrom><LastModifiedTo></LastModifiedTo></InvQtyRq></FbiMsgsRq></FbiXml>";
        }

        private static string listInventory(string key)
        {
            //return "<ExecuteQueryRq><Query>SELECT part.num AS part, part.description, uom.code AS uomcode,COALESCE((SELECT SUM(qtyonhand.qty) FROM qtyonhand WHERE qtyonhand.partid = part.id), 0) AS qty FROM part LEFT JOIN uom ON part.uomid = uom.id</Query></ExecuteQueryRq>";
            return "<ExecuteQueryRq><Query>SELECT * from company</Query></ExecuteQueryRq>";

        }
    }
}

