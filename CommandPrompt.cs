using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace MiniCommandPrompt {
   /// <summary>
   /// Mini command prompt using FileInfo and DirectoryInfo
   /// </summary>
   class CommandPrompt {

      #region Members

      List<string> mValidCommand = new List<string> () { "cd", "dir", "copy", "del", "exit", "help" };
      Dictionary<string,string> mValidCommands = new Dictionary<string,string>();
      // setting current path
      //FileInfo mCurrentPath = new FileInfo (@"d:\");
      DirectoryInfo mCurrentDirectory = new DirectoryInfo (Directory.GetCurrentDirectory ());
      DirectoryInfo mNewDirectory = null;
      DirectoryInfo[] mDirInfos = null;
      FileInfo[] mFileInfos = null;
      FileInfo[] mNewFileInfos = null;
      string mCmd = "";
      string[] mCmdArgs = { };

      bool mIsInCmd = false;

      #endregion Members

      #region Main

      static void Main (string[] args) {
         
         CommandPrompt cp = new CommandPrompt ();
         //Console.TreatControlCAsInput = true;
         Console.CancelKeyPress += cp.Console_CancelKeyPress;
         Console.Title = "Mini Command Prompt";

         while (true) {
            Console.ResetColor ();
            Console.Write (cp.mCurrentDirectory.FullName + ">>");
            cp.mCmd = Console.ReadLine ();
            if (cp.mCmd == null) { Console.WriteLine (); continue; }
            List<string> tempCmd = new List<string>();
            string[] temp = cp.mCmd.Split (' ');

            // Check for empty commands and convert command into lower case        
            foreach (string cmd in temp) {
               if (cmd == String.Empty) continue;
               tempCmd.Add(cmd.ToLower().Trim());
            }
            cp.mCmdArgs = new string[tempCmd.Count];

            int i = 0;
            foreach (string cmd in tempCmd) {
               cp.mCmdArgs[i++] = cmd;
            }

            // Check for empty commands and convert command into lower case
            if ( cp.mCmdArgs.Length == 0) {
               continue;
            } else {
               cp.mCmdArgs[0] = cp.mCmdArgs[0].ToLower ();
            }

            // check for a valid command
            if (!cp.mValidCommand.Contains (cp.mCmdArgs[0])) {
               Console.WriteLine ("Invalid command");
               continue;
            }

            cp.mCmd = cp.mCmdArgs[0];
            switch (cp.mCmd) {
               case "cd":
                  if (!cp.IsValidCD ()) {
                     continue;
                  }
                  break;

               case "dir":
                  if (!cp.IsValidDIR()) {
                     continue;
                  }
                  break;

               case "copy":
                  Console.ForegroundColor = ConsoleColor.Green;
                  if (!cp.IsValidCOPY ()) {
                     continue;
                  }
                  break;
               case "del":
                  Console.ForegroundColor = ConsoleColor.Red;
                  if (!cp.IsValidDEL ()) {
                     continue;
                  }
                  break;

               case "help":
                  Console.WriteLine ("Version: 1.1");
                  Console.WriteLine ("Author: Naveen Kumar V");
                  Console.WriteLine ("");
                  if (cp.mCmdArgs.Length == 1 || cp.mCmdArgs[1] == "") {
                     foreach (string s in cp.mValidCommand) {
                        Console.WriteLine (s);
                     }
                     continue;
                  } else {

                  }
                  break;
               case "exit":
                  Environment.Exit (0);
                  break;

               default:
                  // break in switch exits the switch scope 
                  break;
            }        
         } // end while
      }

      #endregion Main

      #region Methods

      /// <summary>
      /// Lists files and directories given full path of the directory
      /// </summary>
      /// <returns></returns>
      private bool IsValidDIR () {
         try {
            // Check for minimum number of arguments required
            if (mCmdArgs.Length >= 2) {
               // check whether directory is in same folder or other drive
               if (mCmdArgs[1].Contains (":")) {
                  mNewDirectory = new DirectoryInfo (mCmdArgs[1]);
               } else {
                  mNewDirectory = new DirectoryInfo (Path.Combine (mCurrentDirectory.FullName + mCmdArgs[1]));
               }
               
               if (!mNewDirectory.Exists) {
                  Console.WriteLine ("Invalid Directory");
                  return false;
               }
            } else {
               mNewDirectory = new DirectoryInfo (mCurrentDirectory.FullName);
            }
            Console.WriteLine ("{0}:{1}","Path", mNewDirectory.FullName);
            Console.WriteLine ("---------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine ("[Folder(s)]");
            mDirInfos = mNewDirectory.GetDirectories ();
            if (mDirInfos.Length != 0) {
               foreach (DirectoryInfo di in mDirInfos) {
                  if (!(di.Attributes.ToString ().Contains ("Hidden") || di.Attributes.ToString ().Contains ("System"))) {
                     Console.WriteLine ("\t{0}", di.Name);
                  }
               }
            } else {
               Console.WriteLine ("\tNo Folders");
            }
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine ("[File(s)]");
            mFileInfos = mNewDirectory.GetFiles ();
            if (mFileInfos.Length != 0) {
               Console.WriteLine ("\t{0,-15}{1,-10}", "Size", "Name");
               Console.WriteLine ("\t{0,-15}{1,-10}", "----", "----");
               foreach (FileInfo fi in mFileInfos) {
                  Console.WriteLine ("\t{0,-15}{1,-10}", fi.Length, fi.Name);
               }
            } else {
               Console.WriteLine ("\tNo Files");
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine ("---------------------------------------------------------------------");
            return true;
         } catch (Exception e) {
            // TODO
            Console.WriteLine ("Invalid directory location!");
            return false;
         }
         
      }

      /// <summary>
      /// Changes directory to the specified directory
      /// </summary>
      /// <returns></returns>
      private bool IsValidCD () {
         try {
            // Check for minimum number of arguments required
            if (mCmdArgs.Length >= 2) {
               // if changing to another drive/volume in system, check its existence
               if (mCmdArgs[1].Contains (":")) {
                  DirectoryInfo checkDirectory = new DirectoryInfo (mCmdArgs[1]);
                  if (checkDirectory.Exists) {
                     mCurrentDirectory = checkDirectory;
                  }
                  mNewDirectory = new DirectoryInfo (mCurrentDirectory.FullName);
               } else {
                  mNewDirectory = new DirectoryInfo (mCurrentDirectory + @"\" + mCmdArgs[1]);
               }

               if (mNewDirectory == null || !mNewDirectory.Exists) {
                  Console.WriteLine ("Directory invalid (or) not found");
                  return false;
               }
               // change to specified directory
               mCurrentDirectory = mNewDirectory;
               return true;
            } else {
               Console.WriteLine ("Usage: CD [DRIVE:][DIR_NAME]");
               return false;
            }
         } catch (Exception e) {
            // TODO
            Console.WriteLine ("Not a valid drive or location!");
            return false;
         }
      }

      /// <summary>
      /// Copies file(s) to the destination folder with the given name
      /// </summary>
      /// <returns></returns>
      private bool IsValidCOPY () {
         try {
            if (mCmdArgs.Length >= 2) {

               DirectoryInfo sourceDirectoryInfo = new DirectoryInfo (Path.Combine (mCurrentDirectory.FullName, mCmdArgs[1]));
               DirectoryInfo destinationDirectoryInfo = null;
               // IF FILE IS TO BE COPIED
               FileInfo sourceFileInfo;

               // check if source file name includes the absolute path
               if (mCmdArgs[1].Contains (@"\")) {
                  sourceFileInfo = new FileInfo (mCmdArgs[1]);
               } else {
                  sourceFileInfo = new FileInfo (Path.Combine (mCurrentDirectory.FullName, mCmdArgs[1]));
               }
               
               bool IsOverwriteAllowed = false; ;
               if (File.Exists (sourceFileInfo.FullName)) {
                  //if new destination path/filename is given
                  if (mCmdArgs.Length >= 3) {
                     FileInfo destinationFileInfo = new FileInfo (Path.Combine (mCurrentDirectory.FullName, mCmdArgs[2]));
                     Console.ForegroundColor = ConsoleColor.Yellow;
                     // check if destination is other than PWD(Present Working Directory)
                     //if (destinationFileInfo.FullName.EndsWith (@"\")) {
                     if (Directory.Exists(destinationFileInfo.FullName)) {
                        if (File.Exists (Path.Combine( destinationFileInfo.FullName + @"\" +sourceFileInfo.Name))) {
                           IsOverwriteAllowed = IsFileOverwritable (Path.Combine (destinationFileInfo.FullName + @"\" + sourceFileInfo.Name));
                           if (!IsOverwriteAllowed) return false;
                        }
                        File.Copy (sourceFileInfo.FullName, Path.Combine( destinationFileInfo.FullName + @"\" +sourceFileInfo.Name), IsOverwriteAllowed);
                        Console.WriteLine ("{0} is copied/overwrited to {1}", sourceFileInfo.FullName, Path.Combine( destinationFileInfo.FullName + @"\" +sourceFileInfo.Name));
                     } else {
                        if (File.Exists (destinationFileInfo.FullName)) {
                           IsOverwriteAllowed = IsFileOverwritable (destinationFileInfo.FullName);
                           if (!IsOverwriteAllowed) return false;
                        }
                        File.Copy (sourceFileInfo.FullName, destinationFileInfo.FullName, IsOverwriteAllowed);
                        Console.WriteLine ("{0} is copied/overwrited to {1}", sourceFileInfo.FullName, (destinationFileInfo.FullName));
                     }
                  } else {
                     // if new destination file name is not given, copy to PWD(Present Working Directory)
                     FileInfo destinationFileInfo = new FileInfo (Path.Combine (mCurrentDirectory.FullName, sourceFileInfo.Name));
                     // if source and destionation file infos are same
                     if (sourceFileInfo.FullName.CompareTo (destinationFileInfo.FullName) == 0) {
                        Console.WriteLine ("Same file cannot overwrite on it.");
                        return false;
                     } else {
                        if (File.Exists (destinationFileInfo.FullName + sourceFileInfo.Name)) {
                           IsOverwriteAllowed = IsFileOverwritable (destinationFileInfo.FullName + sourceFileInfo.Name);
                           if (!IsOverwriteAllowed) return false;
                        }
                        File.Copy (sourceFileInfo.FullName, destinationFileInfo.FullName, IsOverwriteAllowed);
                        Console.WriteLine ("{0} is copied/overwrited to {1}", sourceFileInfo.FullName, (destinationFileInfo.FullName + sourceFileInfo.Name));
                     }
                  }
               }

               // IF FILES FROM DIRECTORY IS TO BE COPIED
               else if (Directory.Exists (sourceDirectoryInfo.FullName)) {
                  // if new destination folder is given
                  FileInfo[] fileInfos = sourceDirectoryInfo.GetFiles ();
                  // check if destination folder is in another drive
                  if (mCmdArgs.Length >= 3 && mCmdArgs[2].Contains (":")) {
                     destinationDirectoryInfo = new DirectoryInfo (mCmdArgs[2]);
                  } else if (mCmdArgs.Length == 2) {
                     destinationDirectoryInfo = new DirectoryInfo (Path.Combine (mCurrentDirectory.FullName));
                  } else if (mCmdArgs.Length >= 3) {
                     destinationDirectoryInfo = new DirectoryInfo (Path.Combine (mCurrentDirectory.FullName, mCmdArgs[2]));
                  }
                  // Create destination folder if it does not exist.
                  if (destinationDirectoryInfo.FullName != "" && !Directory.Exists (destinationDirectoryInfo.FullName)) {
                     Directory.CreateDirectory (destinationDirectoryInfo.FullName);
                  }
                  // check whether destination folder exists
                  if (Directory.Exists (destinationDirectoryInfo.FullName)) {
                     mNewDirectory = destinationDirectoryInfo;
                  } else {
                     Directory.CreateDirectory (destinationDirectoryInfo.FullName);
                     mNewDirectory = mCurrentDirectory;
                  }
                  foreach (FileInfo fi in fileInfos) {
                     if (File.Exists (Path.Combine (mNewDirectory.FullName, fi.Name))) {
                        IsOverwriteAllowed = IsFileOverwritable (Path.Combine (mNewDirectory.FullName, fi.Name));
                        if (!IsOverwriteAllowed) continue;
                     }
                     File.Copy (fi.FullName, Path.Combine (mNewDirectory.FullName, fi.Name),IsOverwriteAllowed);
                     Console.WriteLine ("{0} is copied/overwrited to {1}", fi.FullName, Path.Combine (mNewDirectory.FullName, fi.Name));
                  }
               } else {
                  Console.WriteLine ("No File or Directory found in the source path");
               }
            } else {
               Console.WriteLine ("USAGE: copy sourcefilename [destinationfilename]");
               return false;
            }
            return true;
         } catch (Exception e) {
            Console.WriteLine ("Copy failed! Filename or Path is incorrect");
            // TODO
            return false;
         }
      }

      /// <summary>
      /// asks user to overwrite existing file
      /// </summary>
      /// <param name="filename"></param>
      /// <returns></returns>
      bool IsFileOverwritable (string filename) {
         Console.Write ("{0} {1} {2}", "Want to overwrite", filename, "(Y/N)?");
         string option = Console.ReadLine ();
         if (option != null && option.StartsWith ("Y", StringComparison.OrdinalIgnoreCase)) {
            return true;
         }
         if (option == null) Console.WriteLine ();
         return false;
      }

      /// <summary>
      /// Deletes files or folder in the specified location
      /// </summary>
      /// <returns></returns>
      private bool IsValidDEL () {
         try {
            // delete only if its a file
            if (mCmdArgs.Length >= 2) {
               FileInfo sourceFile = null;
               // check if file is in another drive
               if (mCmdArgs[1].Contains (":")) {
                  sourceFile = new FileInfo (Path.Combine (mCmdArgs[1]));
               } else {
                  sourceFile = new FileInfo (Path.Combine (mCurrentDirectory.FullName, mCmdArgs[1]));
               }
               // check for file existence
               if (IsFileDeletable(sourceFile.FullName)) {
                  File.Delete (sourceFile.FullName);
               } else if(Directory.Exists(sourceFile.FullName)) {
                  // Delete files from directory
                  DirectoryInfo sourceDirInfo = new DirectoryInfo (sourceFile.FullName);
                  foreach (FileInfo fi in sourceDirInfo.GetFiles ()) {
                     if (IsFileDeletable (fi.FullName)) {
                        File.Delete (fi.FullName);
                     }
                  }
               } 
               else {
                  Console.WriteLine ("Invalid File");
                  return false;
               }
            } else {
               Console.WriteLine ("USAGE: DEL FILENAME/FOLDER");
               return false;
            }
            return true;
         } catch (Exception e) {
            Console.WriteLine ("Delete failed!");
            return false;
         }
      }

      /// <summary>
      /// asks user to delete file
      /// </summary>
      /// <param name="filename"></param>
      /// <returns></returns>
      bool IsFileDeletable (string filename) {
         if (File.Exists (filename)) {
            Console.Write ("Want to delete {0}?", filename);
            int choice = Console.ReadLine ()[0];
            if (choice == 'Y' || choice == 'y') {
               return true;
            }
         }
         return false;
      }

      /// <summary>
      /// Prevents Ctrl+C from terminating console
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void Console_CancelKeyPress (object sender, ConsoleCancelEventArgs e) {
         Console.Beep ();
         e.Cancel = true;
      }      
      

      #endregion Methods
   }
}
