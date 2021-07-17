using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PD2_script_data
{
    using System;

    using DieselScriptData;
    using System.IO;
    using Newtonsoft.Json;
    using System.Runtime.Serialization.Formatters;
    using System.Xml.Serialization;

    public partial class MainForm : Form
    {
        DieselScriptData scriptdata = new DieselScriptData();
        
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Diesel Script(*.*)|*.*";
            ofd.CheckFileExists = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                scriptdata = new DieselScriptData(ofd.FileName);

                string json = JsonConvert.SerializeObject(scriptdata.Root, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
                });
                Console.WriteLine(Path.Combine(Path.GetDirectoryName(ofd.FileName) , Path.GetFileNameWithoutExtension(ofd.FileName) + ".json"));
                System.IO.File.WriteAllText(Path.Combine(Path.GetDirectoryName(ofd.FileName), Path.GetFileNameWithoutExtension(ofd.FileName) + ".json"), json);

                if (this.generateXMLFile.Checked)
                {
                    XMLParser xmlparse = new XMLParser(Path.GetExtension(ofd.FileName).Replace(".", ""), scriptdata.Root);
                    System.IO.File.WriteAllText(Path.Combine(Path.GetDirectoryName(ofd.FileName), Path.GetFileNameWithoutExtension(ofd.FileName) + ".xml"), xmlparse.rootNode.ToString());
                }

                MessageBox.Show("File was decoded successfully.\r\nDecoded file: " + Path.GetFileNameWithoutExtension(ofd.FileName) + ".json", "Decode Successful");
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {

            var ofd = new OpenFileDialog();
            ofd.Filter = "JSON(*.json)|*.json";
            ofd.CheckFileExists = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if ((Path.GetExtension(ofd.FileName).ToLowerInvariant()).Equals(".xml"))
                {
                    dynamic result;
                    using (StreamReader sr = new StreamReader(ofd.FileName))
                    {
                        XMLParser xmlp = new XMLParser(sr.BaseStream);

                        result = xmlp.getDieselScript();

                        scriptdata = new DieselScriptData();
                        scriptdata.export((Dictionary<string, object>)result, Path.Combine(Path.GetDirectoryName(ofd.FileName), Path.GetFileNameWithoutExtension(ofd.FileName) + "_generated." + this.jsonEncodeTypeCombobox.Text));
                        MessageBox.Show("File was encoded successfully as XML.\r\nEncoded file:" + Path.GetFileNameWithoutExtension(ofd.FileName) + "_generated." + this.jsonEncodeTypeCombobox.Text, "Encode Successful");
                    }
                }
                else
                {
                    dynamic result;
                    using (StreamReader sr = new StreamReader(ofd.FileName))
                    {
                        result = JsonConvert.DeserializeObject<dynamic>(sr.ReadToEnd(), new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All,
                            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
                        });

                        scriptdata = new DieselScriptData();
                        scriptdata.export((Dictionary<string, object>)result, Path.Combine(Path.GetDirectoryName(ofd.FileName), Path.GetFileNameWithoutExtension(ofd.FileName) + "_generated." + this.jsonEncodeTypeCombobox.Text));
                        MessageBox.Show("File was encoded successfully.\r\nEncoded file:" + Path.GetFileNameWithoutExtension(ofd.FileName) + "_generated." + this.jsonEncodeTypeCombobox.Text, "Encode Successful");
                    }
                }
            }
            
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.jsonEncodeTypeCombobox.SelectedIndex = 0;
        }
    }
}
