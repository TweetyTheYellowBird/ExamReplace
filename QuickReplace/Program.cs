using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuickReplace
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Quick replace runner");

            if (args.Length < 2)
            {
                Console.WriteLine("No parameters given first parameter definition file second parameter destination folder");
                return;
            }

            string param_file_location = args[0];
            string destination_folder = args[1];

            Console.WriteLine("Param file location = {0}",param_file_location);
            Console.WriteLine("Destination folder = {0}", destination_folder);

            if (!Directory.Exists(destination_folder))
            {
                Console.WriteLine("Source not exists");
                return;
            }

            if (!File.Exists(param_file_location))
            {
                Console.WriteLine("Parameter file not found");
                return; 
            }

            List<string> file_types = new List<string>();
            Dictionary<string, string> replace_val = new Dictionary<string, string>();
            ReadFileParameter(param_file_location, file_types, replace_val);
            SearchAllFiles(destination_folder, file_types, replace_val,param_file_location);
        }

        /// <summary>
        /// Getting all the files that need insp
        /// </summary>
        /// <param name="param_file_location">the file with the replace parameters</param>
        /// <param name="file_types">list of the files type it expected</param>
        /// <param name="replace_val">%key%=values</param>
        static void ReadFileParameter(string param_file_location, List<string> file_types, Dictionary<string, string> replace_val)
        {
            string[] fileslines = File.ReadAllLines(param_file_location);
            string [] files_definitions = fileslines[0].Split('|');
            foreach(string file_type in files_definitions)
            {
                Console.WriteLine("File type: {0}", file_type);
                file_types.Add(file_type);
            }
            for(int i=1;i<fileslines.Length;i++)
            {
                string[] splittedval = fileslines[i].Split('=');
                replace_val.Add(splittedval[0], splittedval[1]);
            }
        }

        /// <summary>
        /// Method that replace the text in the file
        /// </summary>
        /// <param name="filePath">the file for inspection</param>
        /// <param name="searchText">the replaced text</param>
        /// <param name="replaceText">the value for the search</param>
        static void ReplaceInFile(string filePath, string searchText, string replaceText)
        {

            var content = string.Empty;
            using (StreamReader reader = new StreamReader(filePath))
            {
                content = reader.ReadToEnd();
                reader.Close();
            }

            content = Regex.Replace(content, searchText, replaceText);

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(content);
                writer.Close();
            }
        }
        static void SearchAllFiles(string folder_location, List<string> file_types, Dictionary<string, string> replace_val,string param_file_location)
        {
            FileInfo fi = new FileInfo(param_file_location);
            DirectoryInfo d = new DirectoryInfo(folder_location);
 
            List<FileInfo> allfiles = new List<FileInfo>();
            foreach (string file_type in file_types)
            {
                FileInfo[] Files = d.GetFiles("*." + file_type); //Getting all files
                foreach (FileInfo f in Files)
                    if (!allfiles.Contains(f) && f.FullName != fi.FullName)
                        allfiles.Add(f);
            }

            foreach (FileInfo file in allfiles)
            {
                string contents = File.ReadAllText(file.FullName);
                foreach(KeyValuePair<string,string> val in replace_val)
                {
                    string key = val.Key;
                    if (contents.Contains(key))
                    {
                        ReplaceInFile(file.FullName, key, val.Value);
                    }
                }
            }

            foreach (string folder in Directory.GetDirectories(folder_location, "*", SearchOption.TopDirectoryOnly))
            {
                SearchAllFiles(folder,file_types,replace_val,param_file_location);
            }
        }
    }
}
