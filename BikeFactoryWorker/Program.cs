/*
 * File          : Program.cs
 * Project       : BikeFactory
 * Programmer    : Dustin Brown
 * First Version : November 2018
 * Description   : Controls Workers for simulated BikeFactory
 */

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;

namespace BikeFactoryWorker
{
    class Program
    {
        static Random rand = new Random();

        static void Main(string[] args)
        {
            string WorkerFirstName;
            string WorkerLastName;
            int WorkerType = 0;
            int WorkerID = 0;
            int WorkstationID = 0;
            int task = 0;
            decimal timeMultiplier = 1;

            //setup sql connection
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["BikeFactory"].ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Get user to setup worker
            Console.WriteLine("[Create New Worker]");
            Console.Write("First Name: ");
            WorkerFirstName = Console.ReadLine();
            Console.Write("Last Name: ");
            WorkerLastName = Console.ReadLine();
            Console.WriteLine("Select Worker Type...");
            Console.Write("1. Metal Worker\n2. Paint Worker\n3. Assembler Worker\n4. Transporter\nSelection:");
            while (true)
            {
                int.TryParse(Console.ReadLine(), out WorkerType);
                if (WorkerType < 1 || WorkerType > 4)
                {
                    Console.WriteLine("Invalid Worker Type.");
                    Console.Write("Selection: ");
                }
                else break;

            }

            //additional options for Metal, Paint, and Assembly Workers
            switch (WorkerType)
            {
                case 1: //metal worker
                    Console.WriteLine("Select task for Metal Worker...");
                    Console.Write("1. Create Frame Parts\n2. Create Handlebar Parts\n3. Create Fenders\nSelection: ");
                    break;
                case 2: //paint worker
                    Console.WriteLine("Select task for Paint Worker...");
                    Console.Write("1. Paint Frames\n2. Paint Handlebars\n3. Paint Fenders\nSelection: ");
                    break;
                case 3: //assembly worker
                    Console.WriteLine("Select task for Assembly Worker...");
                    Console.Write("1. Assemble Frames\n2. Assemble Handle Bars\n3. Assemble Bicycles\nSelection: ");
                    break;
                default://transporter
                    break;
            }

            //skip input for transport worker
            while (WorkerType != 4)
            {
                int.TryParse(Console.ReadLine(), out task);
                if (task < 1 || task > 3)
                {
                    Console.WriteLine("Invalid task selected.");
                    Console.Write("Selection: ");
                }
                else break;
            }

            //get time multiplier
            Console.Write("Enter Speed Multiplier (1 for normal speed).\nMultiplier: ");
            while (!decimal.TryParse(Console.ReadLine(), out timeMultiplier))
            {
                Console.WriteLine("Invalid value.");
                Console.Write("Multiplier: ");
            }

            //add worker to database
            //TODO:Create procedure for this
            cmd.CommandText = "INSERT INTO Worker (LastName, FirstName, [Type]) OUTPUT Inserted.ID VALUES('" + WorkerLastName + "','" + WorkerFirstName + "', " + WorkerType + ");";
            con.Open();
            Int32.TryParse(cmd.ExecuteScalar().ToString(), out WorkerID);
            Logger.log("Created Worker " + WorkerFirstName + ", " + WorkerLastName + " ID(" + WorkerID + ")");
            if (WorkerType != 4)
            {
                cmd.CommandText = "EXEC spAssignWorkstation " + WorkerID + ", " + WorkerType;
                Logger.log("Attempting to find Workstation");
                while (!Int32.TryParse(cmd.ExecuteScalar().ToString(), out WorkstationID))
                {
                    Logger.log("Workstation not available, waiting 10 seconds and retrying");
                    Thread.Sleep(10000);
                }
                Logger.log("Worker has been assigned to Workstation with ID: " + WorkstationID);
            }


            if (WorkerType == 1) //metal worker
            {
                if (task == 1)// create frame parts
                {
                    //Create Bins necassary for worker
                    //TODO: fix hard coded PartType IDs
                    cmd.CommandText = "EXEC spMakeBin " + WorkstationID + ", 1, 1, 0";
                    int.TryParse(cmd.ExecuteScalar().ToString(), out int SteelTubeBinIn);
                    cmd.CommandText = "EXEC spMakeBin " + WorkstationID + ", 0, 13, 0";
                    int.TryParse(cmd.ExecuteScalar().ToString(), out int FramePartAOut);
                    cmd.CommandText = "EXEC spMakeBin " + WorkstationID + ", 0, 14, 0";
                    int.TryParse(cmd.ExecuteScalar().ToString(), out int FramePartBOut);
                    cmd.CommandText = "EXEC spMakeBin " + WorkstationID + ", 0, 15, 0";
                    int.TryParse(cmd.ExecuteScalar().ToString(), out int FramePartCOut);
                    cmd.CommandText = "EXEC spMakeBin " + WorkstationID + ", 0, 16, 0";
                    int.TryParse(cmd.ExecuteScalar().ToString(), out int FramePartDOut);
                    cmd.CommandText = "EXEC spMakeBin " + WorkstationID + ", 0, 17, 0";
                    int.TryParse(cmd.ExecuteScalar().ToString(), out int FramePartEOut);
                    cmd.CommandText = "EXEC spMakeBin " + WorkstationID + ", 0, 18, 0";
                    int.TryParse(cmd.ExecuteScalar().ToString(), out int FramePartFOut);
                    cmd.CommandText = "EXEC spMakeBin " + WorkstationID + ", 0, 19, 0";
                    int.TryParse(cmd.ExecuteScalar().ToString(), out int FramePartGOut);
                    cmd.CommandText = "EXEC spMakeBin " + WorkstationID + ", 0, NULL, 1";
                    int.TryParse(cmd.ExecuteScalar().ToString(), out int JunkBinOut);
                    Logger.log("Created Bins for Workstation");

                    while (true)
                    {
                        int partsOut = 0; //keeps track of parts produced
                        int binCount = 0; //number of consumable parts
                        cmd.CommandText = "EXEC spBinCount " + SteelTubeBinIn; //get number of parts in SteelTubeBin
                        binCount = Convert.ToInt32(cmd.ExecuteScalar());
                        while (binCount == 0) //if out of parts wait and try again
                        {
                            Logger.log("Out of Steel Tubes... Idling for 10 Seconds...");
                            Thread.Sleep((int)(10000 / timeMultiplier));
                            binCount = Convert.ToInt32(cmd.ExecuteScalar());

                        }
                        Logger.log("Cutting Tube (60 seconds)"); //start cutting part
                        Thread.Sleep((int)(60000 / timeMultiplier));
                        if (SuccessfulPart(1.0))
                        {
                            Logger.log("Milling Tube (30 seconds"); //mill part
                            Thread.Sleep((int)(30000 / timeMultiplier));
                            if (SuccessfulPart(1.0))
                            {
                                Logger.log("Bending Tube (30 seconds)"); //bend part
                                Thread.Sleep((int)(30000 / timeMultiplier));
                                if (SuccessfulPart(0.5))
                                {
                                    Logger.log("Part Finished"); //parts finished, consume part and output frame peice
                                    cmd.CommandText = "EXEC spTakePart " + SteelTubeBinIn;
                                    cmd.ExecuteNonQuery();
                                    //produce frame parts A-G
                                    //TODO: fix hard coding
                                    switch (partsOut % 7)
                                    {
                                        case 0:
                                            cmd.CommandText = "EXEC spGivePart " + FramePartAOut + ", " + 10;
                                            break;
                                        case 1:
                                            cmd.CommandText = "EXEC spGivePart " + FramePartBOut + ", " + 11;
                                            break;
                                        case 2:
                                            cmd.CommandText = "EXEC spGivePart " + FramePartCOut + ", " + 12;
                                            break;
                                        case 3:
                                            cmd.CommandText = "EXEC spGivePart " + FramePartDOut + ", " + 13;
                                            break;
                                        case 4:
                                            cmd.CommandText = "EXEC spGivePart " + FramePartEOut + ", " + 14;
                                            break;
                                        case 5:
                                            cmd.CommandText = "EXEC spGivePart " + FramePartFOut + ", " + 15;
                                            break;
                                        case 6:
                                            cmd.CommandText = "EXEC spGivePart " + FramePartGOut + ", " + 16;
                                            break;
                                    }
                                    cmd.ExecuteNonQuery();
                                }
                                else //mistake was made, throw in junk
                                {
                                    Logger.log("Worker made mistake... Throwing Part in junk bin");
                                    cmd.CommandText = "EXEC spMovePart " + SteelTubeBinIn + ", " + JunkBinOut;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else//mistake was made, throw in junk
                            {
                                Logger.log("Worker made mistake... Throwing Part in junk bin");
                                cmd.CommandText = "EXEC spMovePart " + SteelTubeBinIn + ", " + JunkBinOut;
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else//mistake was made, throw in junk
                        {
                            Logger.log("Worker made mistake... Throwing Part in junk bin");
                            cmd.CommandText = "EXEC spMovePart " + SteelTubeBinIn + ", " + JunkBinOut;
                            cmd.ExecuteNonQuery();
                        }
                    }

                }
                else if (task == 2) //create handle bars
                {
                    //Create Bins necassary for worker
                    //TODO: fix hard coded PartType IDs
                    cmd.CommandText = "EXEC spMakeBin " + WorkstationID + ", 1, 1, 0";
                    int.TryParse(cmd.ExecuteScalar().ToString(), out int SteelTubeBinIn);
                    cmd.CommandText = "EXEC spMakeBin " + WorkstationID + ", 0, 18, 0";
                    int.TryParse(cmd.ExecuteScalar().ToString(), out int HandlebarPartAOut);
                    cmd.CommandText = "EXEC spMakeBin " + WorkstationID + ", 0, 19, 0";
                    int.TryParse(cmd.ExecuteScalar().ToString(), out int HandlebarPartBOut);
                    cmd.CommandText = "EXEC spMakeBin " + WorkstationID + ", 0, NULL, 1";
                    int.TryParse(cmd.ExecuteScalar().ToString(), out int JunkBinOut);
                    while (true)
                    {
                        int partsOut = 0; //keeps track of parts produced
                        int binCount = 0; //number of consumable parts
                        cmd.CommandText = "EXEC spBinCount " + SteelTubeBinIn; //get number of parts in SteelTubeBin
                        binCount = Convert.ToInt32(cmd.ExecuteScalar());
                        while (binCount == 0) //if out of parts wait and try again
                        {
                            Logger.log("Out of Steel Tubes... Idling for 10 Seconds...");
                            Thread.Sleep((int)(10000 / timeMultiplier));
                            binCount = Convert.ToInt32(cmd.ExecuteScalar());

                        }
                        Logger.log("Cutting Tube (60 seconds)"); //start cutting part
                        Thread.Sleep((int)(60000 / timeMultiplier));
                        if (SuccessfulPart(1.0))
                        {
                            Logger.log("Milling Tube (30 seconds"); //mill part
                            Thread.Sleep((int)(30000 / timeMultiplier));
                            if (SuccessfulPart(1.0))
                            {
                                Logger.log("Bending Tube (30 seconds)"); //bend part
                                Thread.Sleep((int)(30000 / timeMultiplier));
                                if (SuccessfulPart(0.5))
                                {
                                    Logger.log("Part Finished"); //parts finished, consume part and output frame peice
                                    cmd.CommandText = "EXEC spTakePart " + SteelTubeBinIn;
                                    cmd.ExecuteNonQuery();
                                    //produce frame parts A-G
                                    //TODO: fix hard coding
                                    switch (partsOut % 2)
                                    {
                                        case 0:
                                            cmd.CommandText = "EXEC spGivePart " + HandlebarPartAOut + ", " + 19;
                                            break;
                                        case 1:
                                            cmd.CommandText = "EXEC spGivePart " + HandlebarPartBOut + ", " + 20;
                                            break;
                                    }
                                    cmd.ExecuteNonQuery();
                                }
                                else //mistake was made, throw in junk
                                {
                                    Logger.log("Worker made mistake... Throwing Part in junk bin");
                                    cmd.CommandText = "EXEC spMovePart " + SteelTubeBinIn + ", " + JunkBinOut;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else//mistake was made, throw in junk
                            {
                                Logger.log("Worker made mistake... Throwing Part in junk bin");
                                cmd.CommandText = "EXEC spMovePart " + SteelTubeBinIn + ", " + JunkBinOut;
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else//mistake was made, throw in junk
                        {
                            Logger.log("Worker made mistake... Throwing Part in junk bin");
                            cmd.CommandText = "EXEC spMovePart " + SteelTubeBinIn + ", " + JunkBinOut;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                else if (task == 3) //create fenders
                {
                    //Create Bins necassary for worker
                    //TODO: fix hard coded PartType IDs
                    cmd.CommandText = "EXEC spMakeBin " + WorkstationID + ", 1, 2, 0";
                    int.TryParse(cmd.ExecuteScalar().ToString(), out int SheetSteelBinIn);
                    cmd.CommandText = "EXEC spMakeBin " + WorkstationID + ", 0, 23, 0";
                    int.TryParse(cmd.ExecuteScalar().ToString(), out int UnpaintedFender);
                    cmd.CommandText = "EXEC spMakeBin " + WorkstationID + ", 0, NULL, 1";
                    int.TryParse(cmd.ExecuteScalar().ToString(), out int JunkBinOut);
                    while (true)
                    {
                        int binCount = 0; //number of consumable parts
                        cmd.CommandText = "EXEC spBinCount " + SheetSteelBinIn; //get number of parts in SheetSteelBin
                        binCount = Convert.ToInt32(cmd.ExecuteScalar());
                        while (binCount == 0) //if out of parts wait and try again
                        {
                            Logger.log("Out of Steel Tubes... Idling for 10 Seconds...");
                            Thread.Sleep((int)(10000 / timeMultiplier));
                            binCount = Convert.ToInt32(cmd.ExecuteScalar());

                        }

                        Logger.log("Bending Tube (45 seconds)"); //bend part
                        Thread.Sleep((int)(45000 / timeMultiplier));
                        if (SuccessfulPart(2.0))
                        {
                            Logger.log("Part Finished"); //parts finished, consume part and output fender
                            cmd.CommandText = "EXEC spTakePart " + SheetSteelBinIn;
                            cmd.ExecuteNonQuery();
                            //produce part
                            //TODO: fix hard coding
                            cmd.CommandText = "EXEC spGivePart " + UnpaintedFender + ", " + 23;
                            cmd.ExecuteNonQuery();
                        }
                        else//mistake was made, throw in junk
                        {
                            Logger.log("Worker made mistake... Throwing Part in junk bin");
                            cmd.CommandText = "EXEC spMovePart " + SheetSteelBinIn + ", " + JunkBinOut;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            else if (WorkerType == 2) //paint worker
            {
                if (task == 1)//paint frame
                {

                }
                else if (task == 2) //paint handlebars
                {

                }
                else if (task == 3) //paint fenders
                {

                }
            }
            else if (WorkerType == 3) //assembly worker
            {
                if (task == 1)//Assemble Frame
                {

                }
                else if (task == 2) //Assemble Handlebars
                {

                }
                else if (task == 3) //Assemble Bicycles
                {

                }
            }
            else if (WorkerType == 4) //transporter
            {
                while (true)
                {
                    //get all raw material input bins
                    cmd.CommandText = "SELECT ID FROM Bin where InOut =1 AND PartType < 10;";
                    SqlDataReader dr = cmd.ExecuteReader();
                    List<int> binID = new List<int>();
                    while (dr.Read())
                    {
                        binID.Add(Convert.ToInt32(dr["ID"]));

                    }
                    dr.Close();
                    //for each bin
                    foreach (int i in binID)
                    {
                        cmd.CommandText = "EXEC spBinCount " + i; //get number of parts in bin
                        if (Convert.ToInt32(cmd.ExecuteScalar()) == 0) //if bin is empty
                        {
                            cmd.CommandText = "SELECT PartType FROM Bin WHERE ID = " + i; //check which parts are needed for bin
                            int partType = Convert.ToInt32(cmd.ExecuteScalar());
                            for (int j = 0; j < 50; j++) //add 50 parts to bin
                            {
                                cmd.CommandText = "INSERT INTO Part(PartType) OUTPUT INSERTED.ID VALUES (" + partType + ")";
                                int partID = Convert.ToInt32(cmd.ExecuteScalar());
                                cmd.CommandText = "INSERT INTO PartMap (Part, Bin) VALUES (" + partID + ", " + i + ")";
                                cmd.ExecuteNonQuery();
                            }
                            Logger.log("Refilled bin " + binID);
                            break;
                        }
                    }
                    Logger.log("Sleeping for 30 seconds");
                    Thread.Sleep((int)(30000 / timeMultiplier));
                }
            }

            Console.ReadLine();
            con.Close();

        }

        // METHOD        : SuccessfulPart
        // DESCRIPTION   : Decides whether a part was made successfully based on its failrate
        // PARAMETERS    : double failrate - the chance the part has to be made with error
        // RETURNS       : bool - whether the part was made successfully or not
        static bool SuccessfulPart(double failrate)
        {
            bool ret = false;
            if (rand.Next(0, 10000) > (int)(failrate * 100)) //if random number is larger than fail rate
            {
                ret = true; //part was made successfully
            }
            else
            {
                ret = false; //part was made with error
            }
            return ret;
        }
    }
}
