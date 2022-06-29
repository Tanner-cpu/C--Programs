/*
 * Name: Tanner Boswell
 * Class: 490-01
 * Assignmnet: Program 2
 * Date: 04-11-2022
*/ 

using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace Program2
{
    /*
     * This program is used as practice for threads in C#. The program creates two consumer threads and one producer thread. They are 
     * started around the same time and all must share a queue. They all have the ability to lock the queue to access what they need. 
     * The producer randomly adds a random amount to the queue three times, and then calls a queuewatcher method. The consumers repeatedly take 
     * an item from the queue and goes to sleep to "proceess" it. They continue this until the queue is empty and the queuewatcher lets 
     * them know nothing else will be added to the queue. 
    */
    class Program
    {
        static Queue<int> nodes = new Queue<int>();
        static Random rand = new Random();
        static public int processCount = 0;
        static public int produceCount = 0;
        static public bool queueWatchbool = false;
        static public bool drained = false;
       
         
        /*
         * This method is used as the producer thread. Once called from the main program, it locks the shared queue and generates 
         * a random number between 5 and 25 which acts as the qauntity of additional processes that shall be added to the queue. 
         * It adds the additional processes and goes to sleep for a random amount of time. Once it wakes up, it repeats this process 
         * for a total of three times. It then makes the queueWatcher method available and returns to the main program. 
        */
        static void Producer()
        { 
            bool finished = false; //remains false until producer runs 3 times

            Console.WriteLine("Starting Producer...");

            while(!finished) //continues method until finished
            {
                lock(nodes) //lock the queue 
                {
                    if(!finished) //continues until finished
                    {     
                        int additionalProcesses = rand.Next(5, 25); // generate random number

                        for(int j = 0; j < additionalProcesses; j++) 
                        {
                            nodes.Enqueue(1); //add that amount to the queue 
                        }

                    Console.WriteLine("\nProducer has added " + additionalProcesses + " more nodes to the list"); //update user
                    Console.WriteLine("Producer thinks there are " + nodes.Count + " nodes in queue\n"); 
                    produceCount += 1; //increase number of times producer has added to the list
                        }

                    
                } 

                //If the producer has added to the list 3 times, make queuewatcher available and finish 
                if(produceCount == 3)
                {
                    Console.WriteLine("Producer has completed its tasks...\n");
                    queueWatchbool = true; 
                    Console.WriteLine("Queuewatcher has started...\n");
                    finished = true;
                }

                Thread.Sleep(rand.Next(7000)); //Sleep for a random amount of time 
     
            }   
             return;   
            }


        /*
         * This method is used to check the amount of items still in the queue. It switches a public variable 
         * once the queue is empty to notify other methods.
        */

        static bool queueWatcher()
        {
            if(nodes.Count == 0) //If the queue is empty, change the value of drained
            {
                drained = true; 
            }

            return drained; //return drained value
        }

        /*
         * This method is used as the consumer thread. Once called, it sets the header for the specific thread ID. 
         * It, then, locks the queue and checks if anything is in the queue. If there is something in the queue, it takes the 
         * first item, unlocks the queue, and keeps track of the amount of processes that individual thread has taken. It goes to 
         * sleep for a certain amount of time and comes back to notify the user that the process has been completed.  
         * It continues to do that until nothing is in the queue. If nothing is in the queue and the queuewatcher has not 
         * yet been activated, then the thread goes to sleep for a predetermined amount of time. If nothing is in the queue and 
         * the queuewatcheer has been activated, then it notifies the amount of processes that individual thread completed and 
         * returns back to the main program.
        */
        public static void Consumer(int idNum, int nSleep)
        {
            //variables used for logic
            String header = "";
            bool finished = false;
            int processTaken = 0;
            bool executed = false;
            int total = 0; 
            
            for (int i = 1; i < idNum; i++) //sets a header value for formatting 
            {
                header = header +  "       "; 
            }
            Console.WriteLine(header + "CPU " + idNum + " is starting..."); //notifies the user
            int value; 

            while(!finished) //continues until finished
            {
                executed = false; //it continues to loop through and must check to see if anything processes were taken 

                lock(nodes) //locks queue
                {
                
                    if(!finished) //continuees until finished
                    {
                        if (nodes.Count != 0) //if there is something in the queue, take the first item from the queue and update values
                        {
                            value = nodes.Dequeue();
                            processCount += 1;
                            total += 1; 

                            executed = true; 
                            processTaken = processCount; //strategy used so threads are not taking the same process
                            
                        }
                    }
                   
                }
            if(executed == true) //if a process was taken in this specific run, go to sleep and record the amount of time it took to process
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start(); 
                    Thread.Sleep(nSleep);
                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    long nProcessID = Process.GetCurrentProcess().Id;
                    
                    //notify the user which process was completed 
                    Console.WriteLine(header + "CPU " + idNum + " finished Process:" + processTaken + "(" + ts.Milliseconds + " ms) at " + nProcessID);
                }

            //If there is nothing in the queue but the producer is not finished, go to sleep for a small amount of time and come back to check again 
            if(nodes.Count == 0 && queueWatchbool != true)
                {
                    Console.WriteLine(header + "CPU " + idNum + " is idle");
                    Thread.Sleep(5000);  
                }
            else if(queueWatchbool == true) //Else if the queuewatcher is activated and there is nothing left, then notify user and return 
                {
                    bool queueStatus = queueWatcher(); 
                    if(queueStatus == true)
                    {
                        Console.WriteLine(header + "CPU " + idNum + " exiting - completed " + total + " processes...");
                        finished = true;
                    }
                }
            
            }

            return; 
        }

        static void P1()
        {
            Producer(); 
        }  

        static void C1()
        {
            Consumer(1, 100); 
        }

        static void C2()
        {
            Consumer(2, 100); 
        }

        //Main program that starts threads and waits for them to complete
        static void Main(string[] args)
        {
            Thread consumer1 = new Thread(new ThreadStart(C1));//Spawn instance of one consumer
            Thread consumer2 = new Thread(new ThreadStart(C2));//Spawn instance of another consumer
            Thread producer1 = new Thread(new ThreadStart(P1));//Spawn instance of producer

            //Start the threads
            consumer1.Start();
            consumer2.Start();
            producer1.Start(); 

            //Join and wait for them to return 
            consumer1.Join();
            consumer2.Join();
            producer1.Join();

            //Notify the user that the threads are completed 
            Console.Write("\n\nMain program exiting");
            Console.ReadKey(); 

        }
    }
}
