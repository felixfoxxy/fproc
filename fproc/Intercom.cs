using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fproc
{
    public delegate void MessageReceived(string message);

    public class IntercomClient
    {
        public NamedPipeClientStream pipeClient;
        public StreamReader reader;
        public StreamWriter writer;
        public Thread MessageLoop;

        public event MessageReceived OnMessageReceived;

        public bool Connect(string pipeName, string serverName = ".", int timeout = -1)
        {
            try
            {
                pipeClient = new NamedPipeClientStream(serverName, pipeName, PipeDirection.InOut);
                reader = new StreamReader(pipeClient);
                writer = new StreamWriter(pipeClient);

                if(timeout == -1)
                    pipeClient.Connect();
                else
                    pipeClient.Connect(timeout);

                MessageLoop = new Thread(() => MessageLoopFunc());
                MessageLoop.Start();

                return pipeClient.IsConnected;
            }catch (Exception) { return false; }
        }

        public void MessageLoopFunc()
        {
            while (true)
            {
                try
                {
                    if (pipeClient == null)
                        return;

                    if (pipeClient.IsConnected)
                    {
                        string line = reader.ReadLine();
                        if (line != null && !string.IsNullOrEmpty(line))
                            OnMessageReceived?.Invoke(line);
                    }
                    else
                        return;
                }
                catch (Exception) { }
            }
        }

        public bool SendMessage(string message)
        {
            try
            {
                writer.WriteLine(message);
                writer.Flush();
                return true;
            }
            catch (Exception) { return false; }
        }

        public bool Disconnect()
        {
            try
            {
                if (pipeClient == null && MessageLoop == null)
                    return true;

                MessageLoop.Abort();
                MessageLoop = null;

                pipeClient.Close();
                pipeClient.Dispose();
                pipeClient = null;

                return true;
            }
            catch (Exception) { return false; }
        }
    }

    public class IntercomServer
    {
        public NamedPipeServerStream pipeServer;
        public StreamReader reader;
        public StreamWriter writer;
        public Thread MessageLoop;

        string nig = "";

        public event MessageReceived OnMessageReceived;

        public bool Start(string pipeName, bool HandleConnection = true)
        {
            try
            {
                pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut);
                reader = new StreamReader(pipeServer);
                writer = new StreamWriter(pipeServer);

                MessageLoop = new Thread(() => MessageLoopFunc());
                MessageLoop.Start();

                nig = pipeName;
                return true;
            }catch (Exception ex){
                Console.WriteLine(ex.StackTrace);
                return false; 
            }
        }

        public void MessageLoopFunc()
        {
            while (true)
            {
                Console.WriteLine("nogger - " + nig);
                try
                {
                    if (pipeServer == null)
                        continue;

                    if (pipeServer.IsConnected)
                    {
                        string line = reader.ReadLine();
                        if(line != null && !string.IsNullOrEmpty(line))
                            OnMessageReceived?.Invoke(line);
                    }
                    else
                        AwaitConnection();
                }catch (Exception ex) {
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        public bool SendMessage(string message)
        {
            try
            {
                if (!pipeServer.IsConnected)
                    return false;

                writer.WriteLine(message);
                //writer.Flush();
                return true;
            }
            catch (Exception ex) {
                Console.WriteLine(ex.StackTrace); 
                return false; 
            }
        }

        public void AwaitConnection()
        {
            pipeServer.WaitForConnection();
        }

        public bool Stop() {
            try {
                if (pipeServer == null && MessageLoop == null)
                    return true;

                MessageLoop.Abort();
                MessageLoop = null;

                pipeServer.Close();
                pipeServer.Disconnect();
                pipeServer.Dispose();
                pipeServer = null;

                return true;
            }
            catch(Exception ex) {
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }

        public bool Running()
        {
            return pipeServer != null && MessageLoop != null;
        }
    }
}
