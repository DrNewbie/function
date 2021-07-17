using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PD2_script_data
{
    class XMLParserNode
    {
        public String meta;
        public HashSet<XMLParserNode> children = new HashSet<XMLParserNode>();
        public Dictionary<String, object> attributes = new Dictionary<string, object>();
        public int indent = 0;
        public int index = -1;
        public String data;

        public XMLParserNode()
        {
        }

        public XMLParserNode(string meta, string data, int index, int indent = 0)
        {
            this.indent = indent;
            this.index = index;
            this.meta = meta;
            this.data = data;
        }

        public XMLParserNode(string meta, float[] data, int index, int indent = 0)
        {
            this.indent = indent;
            this.meta = meta;
            this.index = index;
            this.data = "";
            foreach(float val in data)
            {
                this.data += (this.data == "" ? "" : " ") + val.ToString();
            }
        }

        public XMLParserNode(string meta, float data, int index, int indent = 0)
        {
            this.indent = indent;
            this.index = index;
            this.meta = meta;
            this.data = data.ToString();
        }

        public XMLParserNode(string meta, bool data, int index, int indent = 0)
        {
            this.indent = indent;
            this.index = index;
            this.meta = meta;
            this.data = data.ToString();
        }

        public XMLParserNode(string meta, Dictionary<string, object> data, int index = -1, int indent = 0)
        {
            this.indent = indent;
            this.index = index;
            
            if (data.ContainsKey("_meta"))
            {
                this.meta = data["_meta"].ToString();
            }
            else
            {
                this.meta = meta;
            }

            foreach(KeyValuePair<string, object> kvp in data)
            {
                if (kvp.Key.Equals("_meta"))
                    continue;

                int newindex;
                if (Int32.TryParse(kvp.Key, out newindex))
                {
                    if (kvp.Value is Dictionary<string, object>)
                        this.children.Add(new XMLParserNode("table", kvp.Value as Dictionary<string, object>, newindex, this.indent + 1));
                    else if (kvp.Value is float)
                        this.children.Add(new XMLParserNode("value_node", (float)kvp.Value, newindex, this.indent + 1));
                    else if (kvp.Value is string)
                        this.children.Add(new XMLParserNode("value_node", (string)kvp.Value, newindex, this.indent + 1));
                    else if (kvp.Value is bool)
                        this.children.Add(new XMLParserNode("value_node", (bool)kvp.Value, newindex, this.indent + 1));
                    else if (kvp.Value is float[])
                        this.children.Add(new XMLParserNode("value_node", (float[])kvp.Value, newindex, this.indent + 1));
                    else
                        Console.WriteLine("No option for " + kvp.Value);

                }
                else
                {
                    if (kvp.Value is Dictionary<string, object>)
                        this.children.Add(new XMLParserNode(kvp.Key, kvp.Value as Dictionary<string, object>, -1, this.indent + 1));
                    else
                        this.attributes.Add(kvp.Key, kvp.Value);
                }
            }
        }

        public XMLParserNode(StreamReader data)
        {
            bool closed = false;

            string line = data.ReadLine();
            int line_pos = 0;

            if (String.IsNullOrWhiteSpace(line))
                return;

            for (; line_pos < line.Length; line_pos++)
            {
                if(line[line_pos] == '<')
                {
                    line_pos++;
                    break;
                }
            }

            if (!closed && line_pos < line.Length && line[line_pos] == '/')
            {
                closed = true;    
                line_pos++;
            }

            StringBuilder meta = new StringBuilder();

            for (; line_pos < line.Length; line_pos++)
            {
                if(line[line_pos] != ' ' && line[line_pos] != '=' && line[line_pos] != '/' && line[line_pos] != '>')
                {
                    meta.Append(line[line_pos]);
                }
                else
                {
                    line_pos++;
                    break;
                }
            }

            this.meta = meta.ToString();

            if(line_pos < line.Length)
            {
                StringBuilder attrib_name = new StringBuilder();
                StringBuilder attrib_data = new StringBuilder();

                for (; line_pos < line.Length; line_pos++)
                {
                    if(line[line_pos] == '/' || line[line_pos] == '>')
                    {
                        break;
                    }

                    if(line[line_pos] == ' ')
                    {
                        continue;
                    }
                        
                    for (; line_pos < line.Length; line_pos++)
                    {
                        if(line[line_pos] != '=')
                        {
                            attrib_name.Append(line[line_pos]);
                        }
                        else
                        {
                            line_pos++;
                            break;
                        }
                    }

                    if(line[line_pos] == '"')
                        line_pos++;

                    for (; line_pos < line.Length; line_pos++)
                    {
                        if(line[line_pos] != '"')
                        {
                            attrib_data.Append(line[line_pos]);
                        }
                        else
                        {
                            break;
                        }
                    }

                    Boolean data_bool;
                    if(Boolean.TryParse(attrib_data.ToString(), out data_bool))
                    {
                        this.attributes.Add(attrib_name.ToString(), data_bool);
                        attrib_name.Clear();
                        attrib_data.Clear();
                        continue;
                    }

                    float data_float;
                    if(float.TryParse(attrib_data.ToString(), out data_float))
                    {
                        this.attributes.Add(attrib_name.ToString(), data_float);
                        attrib_name.Clear();
                        attrib_data.Clear();
                        continue;
                    }

                    List<float> data_floats = new List<float>();
                    bool isFloatArray = true;
                    if(attrib_data.ToString().Split(' ').Length > 1)
                    {
                        string[] splits = attrib_data.ToString().Split(' ');
                            
                        foreach(String spl in splits)
                        {
                            float test_out;
                            if(float.TryParse(spl, out test_out))
                            {
                                data_floats.Add(test_out);
                            }
                            else
                            {
                                isFloatArray = false;
                            }

                        }

                        if(isFloatArray)
                        {
                            this.attributes.Add(attrib_name.ToString(), data_floats.ToArray());
                            attrib_name.Clear();
                            attrib_data.Clear();
                            continue;
                        }
                    }

                    this.attributes.Add(attrib_name.ToString(), attrib_data.ToString());
                    attrib_name.Clear();
                    attrib_data.Clear();

                }
            }

            if (!closed && line_pos < line.Length && line[line_pos] == '/')
            {
                closed = true;
                line_pos++;
            }

            if (!closed)
            {
                XMLParserNode child;
                while (!((child = new XMLParserNode(data)).meta).Equals(this.meta))
                    this.children.Add(child);
            }
        }

        public Dictionary<string, object> ToDieselScript()
        {
            Dictionary<string, object> toreturn = new Dictionary<string, object>();

            toreturn.Add("_meta", this.meta);

            foreach (var attr in this.attributes)
                toreturn.Add(attr.Key, attr.Value);

            int element_count = 1;
            int additional_count = 1;

            foreach (XMLParserNode child in this.children)
            {
                String temp_meta = ( child.meta.ToLowerInvariant().Equals("value_node") ? (element_count++).ToString() : child.meta );

                if (toreturn.ContainsKey(temp_meta))
                    toreturn.Add((additional_count++).ToString(), child.ToDieselScript());
                else
                    toreturn.Add(temp_meta, child.ToDieselScript());
            }

            return toreturn;
        }

        public override string ToString()
        {
            StringBuilder indentation = new StringBuilder();
            for (int i = 0; i < this.indent; i++)
                indentation.Append("\t");

            StringBuilder sb = new StringBuilder();

            sb.Append(indentation.ToString() + "<" + this.meta);

            if (this.index != -1)
            {
                //sb.Append(" index=\"" + this.index + "\"");
            }

            foreach (KeyValuePair<string, object> kvp in this.attributes)
            {
                if (kvp.Value is float[])
                {
                    sb.Append(" " + kvp.Key + "=\"");

                    foreach(float f in (kvp.Value as float[]))
                        sb.Append(f+" ");

                    sb.Remove(sb.Length-1, 1);

                    sb.Append("\"");

                }
                else
                    sb.Append(" " + kvp.Key + "=\"" + kvp.Value + "\"");
            }

            if (this.children.Count > 0)
            {
                sb.Append(">\r\n");
                foreach (XMLParserNode child in this.children)
                {
                    sb.Append(child);
                }

                sb.Append(indentation.ToString() + "</" + this.meta + ">\r\n");
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(this.data))
                {
                    //sb.Append(">");
                    //sb.Append(this.data + "</" + this.meta + ">\r\n");
                    sb.Append(" value=\"" + this.data + "\"/>\r\n");
                }
                else
                {
                    sb.Append("/>\r\n");
                    //sb.Append(">\r\n");
                    //sb.Append(indentation.ToString() + "</" + this.meta + ">\r\n");
                }
            }

            return sb.ToString();
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is XMLParserNode)
                return this.ToString().Equals((obj as XMLParserNode).ToString());

            return false;
        }
    }
    
    class XMLParser
    {

        public XMLParserNode rootNode;

        public XMLParser(Stream data)
        {
            StreamReader sr = new StreamReader(data);
            rootNode = new XMLParserNode(sr);
        }

        public XMLParser(string root_meta, Dictionary<string, object> root)
        {
            rootNode = new XMLParserNode(root_meta, root);
        }

        public Dictionary<string, object> getDieselScript()
        {
            Dictionary<string, object> toreturn = new Dictionary<string, object>();
            toreturn = this.rootNode.ToDieselScript();

            return toreturn;
        }
    }
}
