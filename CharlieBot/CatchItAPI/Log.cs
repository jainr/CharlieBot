/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
********************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Reflection;
//using Windows.Storage;
//using Windows.Storage.Streams;

namespace POka.Diagnostics
{
    static class Log
    {
        private const string _logFilename               = "logFile.txt";
  //      static private IOutputStream _outputStream      = null;
  //      static private StorageFile _logFile             = null;
  //      static private IRandomAccessStream _writeStream = null;
        static private string _asmInfo                  = "";
        static private Object lockObject                = new Object();
        static private bool bFileOpen                   = false;
        
        static private void Dispose()
        {
            lock (lockObject)
            {
#if false
                if (_writeStream != null)
                {
                    _writeStream.Dispose();
                    _writeStream = null;
                }
                if (_outputStream != null)
                {
                    _outputStream.Dispose();
                    _outputStream = null;
                }
                if (_logFile != null)
                {
                    _logFile = null;
                }
#endif  
            }
        }
        static public void WriteLine(string methodName, string logMessage)
        {
#pragma warning disable 4014
            WriteLineAsync(methodName, logMessage);
#pragma warning restore 4014
        }
        /// <summary> 
        /// Asynchronously write a string to a file 
        /// </summary> 
        /// <param name="methodName">Method name calling WriteLine</param>
        /// <param name="logMessage">Text to write</param> 
        /// <returns>Task/ bool, true if successfull</returns> 
        static public async Task<bool> WriteLineAsync(string methodName, string logMessage)
        {
            return false;
#if false
            bool bReturn = false;
            DataWriter dataWriter = null;
 
            try
            {
                // Initialize Assembly Info
                if (_asmInfo.Length == 0) _asmInfo = GetAssemblyInfo();

                string Message = string.Format("{1} : {1:fff}\t({0})\t({2})\t{3}\r\n", _asmInfo, DateTime.UtcNow, methodName, logMessage);
                Debug.WriteLine(Message);

                // Initialize output stream
                if (_outputStream == null)
                {
                    // First time using static object, need to open the file.
                    await OpenAsync(_logFilename, CreationCollisionOption.OpenIfExists);
 
                    if (bFileOpen == false)
                    {                        
                        // Clean up anything resources                     
                        Dispose();

                        // Try creating a file with a unique name
                        await OpenAsync(_logFilename, CreationCollisionOption.GenerateUniqueName);
                    }
                }
                lock (lockObject)
                {
                    if (_outputStream != null)
                    {
                        // Log File open and ready to use.
                        dataWriter = new DataWriter(_outputStream);

                        // Only allow one thread at a time to wirte to the stream.
                        dataWriter.WriteString(Message);

                        bReturn = true;
                    }
                    
                }
                // Commit changes to file
                await dataWriter.StoreAsync();
                await _outputStream.FlushAsync();
            }
            catch (Exception e)
            {
                // File excpetion!
                string errMsg = "Log.WriteLine() Exception Caught = " + e.Message;
                Debug.Assert(false, errMsg);
                // Debugger.Break();
                Debug.WriteLine(errMsg);
            }

            // Close file and clean up resources
            Dispose();
            
            return bReturn;
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string FormatExceptionMsg(Exception e)
        {
            string errMsg2 = "";
            // check for an inner exception
            if (e.InnerException != null)
            {
                errMsg2 = e.InnerException.Message;
            }
            return string.Format("Caught Exceptions with Message({0}), InnerMessage({1})", e, errMsg2);
        }
        public static string FormatSigniture(object obj, string method)
        {
            return obj.GetType().ToString() + "." + method;
        }
        // *********************************************************************************
        // *********************************************************************************
        #region Private Methods
        /// <summary> 
        /// Asynchronously opens file to write to 
        /// </summary> 
        /// <param name="storageFile">StorageFile to write text to</param> 
        /// <returns>Task/ IOutputStream if used with await</returns> 
#if false
        static private async Task OpenAsync(string logFilename, CreationCollisionOption options)
        {
            bFileOpen = false;
            StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            
            try
            {
                // Create the log file or open it if exists
                _logFile = await localFolder.CreateFileAsync(logFilename, options);
 
                if (_logFile != null)
                {
                    // Open the file for read/write
                    _writeStream = await _logFile.OpenAsync(FileAccessMode.ReadWrite);
                    if (_writeStream != null)
                    {
                        // Sync the output stream to write to the end of the file.
                        _outputStream = _writeStream.GetOutputStreamAt(_writeStream.Size);
                        bFileOpen = true;
                    }
                }
            }
            catch (Exception e)
            {
                // File excpetion!
                string errMsg = "Log.Open() Exception Caught = " + e.Message;
                Debug.Assert(false, errMsg);
                //Debugger.Break();
                Debug.WriteLine(errMsg);
            } // try/catch
    }
    /// <summary> 
    /// Gets a Assembly info 
    /// </summary> 
    /// <returns> string Full name of assembly </returns> 
    static private string GetAssemblyInfo()
        {            
            string retVal = null;

            try
            {
                TypeInfo t = typeof(Log).GetTypeInfo();

                Assembly asm = t.Assembly;
                string t1 = asm.ToString();

                string[] t2 = t1.Split(',');

                retVal = string.Format("{0}, {1}", t2[0], t2[1]);

            }
            catch (Exception e)
            {
                string errMsg = "Log.GetAssembly() Exception Caught = " + e.Message;
                Debug.Assert(false, errMsg);
                Debug.WriteLine(errMsg);
            }

            return retVal;
        }
#endif
        #endregion
    }
}
