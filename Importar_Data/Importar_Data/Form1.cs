using System;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows.Forms;

//using Interop.Microsoft.Office.Interop.Excel;

namespace Importar_Data
{
    public partial class Form1 : Form
    {
        public int nFlag;
        public DataTable TablaA;
        public DataTable camposA;
        public int nTotalReg;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string[] instancias;
                instancias = instanciasInstaladas();
                foreach (string s in instancias)
                {
                    if (s == "MSSQLSERVER")
                    {
                        C_Servidor.Items.Add("(local)");
                    }
                    else
                    {
                        C_Servidor.Items.Add(@"(local)\" + s);
                    }
                }

                //CARGAR INSTANCIAS DE SERVIDOR EN LA RED LOCAL
                SqlDataSourceEnumerator instance =
                    SqlDataSourceEnumerator.Instance;
                System.Data.DataTable table = instance.GetDataSources();

                // Display the contents of the table.
                Instancias_Remotas(table);

                //conexion con autenticacion windows
                //string sCnn = "Server=" + Servidor + "; database=" + C_Bases_De_Datos.Text + "; User =" + TxtUsuario.Text + "; Password =" + TxtPassword.Text;
            }
            catch(Exception jj)
            {
                MessageBox.Show("Error: " + jj.ToString());
            }
        }

        private void ObtenerHojas(int nFlag)
        {
            //DataSet dsMsExcel = new DataSet();
            DbProviderFactory factory = DbProviderFactories.GetFactory("System.Data.OleDb");
            DataTable worksheets = new DataTable();
            try
            {
                //DataTable worksheets;
                DbConnection connection = factory.CreateConnection();
                string connectionString = "";
                if(nFlag==1)
                    connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + TxtPath.Text + ";Extended Properties=Excel 8.0;";
                else
                    connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;" + ("Data Source=" + (TxtPath.Text + ";Extended Properties=\"Excel 12.0;HDR=YES\""));
                connection.ConnectionString = connectionString;
                connection.Open();
                worksheets = connection.GetSchema("Tables");
                C_Hoja.Items.Clear();
                if (worksheets.Rows.Count > 0)
                {
                    foreach (DataRow row in worksheets.Rows)
                    {
                        C_Hoja.Items.Add(row["TABLE_NAME"].ToString());
                    }
                }
                //ciclo infinito si se setea el index de C_Hoja=0

            }
            catch (Exception j)
            {
                MessageBox.Show("Error:"+j.Message);
            }
            if (worksheets.Rows.Count>0)
                C_Hoja.SelectedIndex = 0;
        }

        private void EliminarFilasVacias()
        {
            try
            {
                int nFlag = 0;
                int nIndice, nTotalEliminados;
                nIndice = nTotalReg = nTotalEliminados = 0;
                foreach (DataRow row in TablaA.Rows)
                {
                    foreach (DataColumn myProperty in TablaA.Columns)
                    {
                        if (row[myProperty].ToString().Trim().Length <= 0 || row[myProperty] == null) // ||
                            nFlag++;
                    }
                    if (nFlag == TablaA.Columns.Count)
                    {
                        TablaA.Rows[nIndice].Delete();
                        nTotalEliminados++;
                        //nIndice--;
                    }
                    nFlag = 0;
                    nIndice++;
                }
                nTotalReg = (TablaA.Rows.Count - nTotalEliminados) + 1;
                //MessageBox.Show("Total Filas:" + TablaA.Rows.Count + " Reg Totales:" + nTotalReg);
            }
            catch (Exception jj)
            {
                
                
            }
        }

        public string ObtenerExtension(string sNombreArchivo, int nFlag)
        {
            string strDocType = "";
            try
            {
                if (sNombreArchivo.Length > 0)
                {
                    int nIndice = sNombreArchivo.LastIndexOf(".");
                    if (nFlag == 0)
                        strDocType = sNombreArchivo.Substring(sNombreArchivo.Length - (sNombreArchivo.Length - nIndice) + 1);
                    else
                        strDocType = sNombreArchivo.Substring(sNombreArchivo.Length - (sNombreArchivo.Length - nIndice));
                }
                else
                    strDocType = "-1";

            }
            catch (Exception jj)
            {

            }
            return strDocType;
        }

        private void CargarHojaDeExcel(string sPath)
        {

            //openFileDialog0.FileName.Trim()
            OleDbConnection DBConnection = new OleDbConnection();
            try
            {
                string SQLString = "";
                switch (ObtenerExtension(sPath,0))
                {
                    case "xls":
                         DBConnection =
                            new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source =" + sPath.Trim() +
                                                ";" + "Extended Properties=\"Excel 8.0;HDR=Yes\"");
                        break;
                    case "xlsx":
                        //  "Provider=Microsoft.ACE.OLEDB.12.0;" + ("Data Source=" + (sPath + ";Extended Properties=\"Excel 12.0;HDR=YES\""))
                        DBConnection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;" + ("Data Source=" + (sPath + ";Extended Properties=\"Excel 12.0;HDR=YES\"")));
                        /*OleDbDataAdapter adp = new OleDbDataAdapter("select * from [" + Table + "]", baglanti);
                        DataSet ds2 = new DataSet();
                        adp.Fill(ds2);
                        dataGridView1.DataSource = ds2.Tables[0];*/
                        break;
                }
                DBConnection.Open();
                SQLString = "SELECT * FROM [" + C_Hoja.Text.Trim() + "]"; //+ "$]";
                OleDbCommand DBCommand = new OleDbCommand(SQLString, DBConnection);
                IDataReader DBReader = DBCommand.ExecuteReader();
                TablaA = new DataTable();
                Grd_Data.DataSource = "";
                TablaA.Load(DBReader);
                //Grd_Data.DataSource = TablaA;
                DBReader.Close();
                //CARGAR CAMPOS DE LA HOJA DE EXCEL
                Lst_Campos_Excel.Items.Clear();
                foreach (DataRow row in TablaA.Rows)
                {
                    //MessageBox.Show(row["Id"].ToString());
                    foreach (DataColumn myProperty in TablaA.Columns)
                    {
                        //Mostrar el nombre y el valor del campo.
                        Lst_Campos_Excel.Items.Add(myProperty.ColumnName);
                        //MessageBox.Show(myProperty.ColumnName + " = " + row[myProperty].ToString());
                    }
                    break;
                }
                //COMPACTAR TABLA ELIMINANDO REGISTROS EN BLANCO
                EliminarFilasVacias();
                Grd_Data.DataSource = TablaA;
                DBConnection.Close();
            }
            catch(Exception jj)
            {
                MessageBox.Show("ERROR: No existe la hoja " + C_Hoja.Text + " en el documento.");
            }
        }

        private void cmdCargarArchivo_Click(object sender, EventArgs e)
        {
            try
            {
                //Abrir Documento
                OpenFileDialog openFileDialog0 = new OpenFileDialog();
                openFileDialog0.Filter = "Office Files|*.xls;*.xlsx";
                openFileDialog0.Title = "Selecciona La Hoja De Calculo A Importar";
                if (openFileDialog0.ShowDialog() == DialogResult.OK)
                {
                    TxtPath.Text = openFileDialog0.FileName.Trim();
                    switch (ObtenerExtension(TxtPath.Text, 0))
                    {
                        case "xls":
                            ObtenerHojas(1);
                            break;
                        case "xlsx":
                            ObtenerHojas(2);
                            break;
                    }
                    CargarHojaDeExcel(openFileDialog0.FileName.Trim());
                }
                TxtTotalRegistros.Text = (nTotalReg - 1).ToString();
            }
            catch (Exception jj)
            {
                

            }
        }

        //OBTENER INSTANCIAS LOCALES
        private string[] instanciasInstaladas()
        {
            try
            {
                Microsoft.Win32.RegistryKey rk;
                rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server", false);
                string[] s;
                s = ((string[])rk.GetValue("InstalledInstances"));
                return s;
            }
            catch (Exception jj)
            {
                

            }
            return null;
        }

        //OBTENER INSTANCIAS EN RED 
        private void Instancias_Remotas(System.Data.DataTable table)
        {
            try
            {
                string sTemp = "";
                foreach (System.Data.DataRow row in table.Rows)
                {
                    if (sTemp.Trim().Length == 0)
                    {
                        C_Servidor.Items.Add(row["ServerName"]);
                        sTemp = row["ServerName"].ToString();
                    }
                    else
                    {
                        if (sTemp != row["ServerName"].ToString())
                            C_Servidor.Items.Add(row["ServerName"]);
                        sTemp = row["ServerName"].ToString();
                    }
                }
            }
            catch (Exception jj)
            {
                

            }
        }

        private String[] basesDeDatos(string instancia)
        {
            // Las bases de datos propias de SQL Server
            string[] basesSys = { "master", "model", "msdb", "tempdb" };
            string[] bases;
            DataTable dt = new DataTable();
            //Cadena de Conexion de SQL Server 2005 Autenticacion SQL
            string sCnn;
            sCnn = "Data Source="+C_Servidor.Text+";Initial Catalog=master;Persist Security Info=True;User ID="+TxtUsuario.Text+";Password="+TxtPassword.Text;
            //Cadena de Conexion de SQL Server 2005 Autenticacion SQL
            //string sCnn = "Server=" + instancia + "; database=master; integrated security=yes";

            // La orden T-SQL para recuperar las bases de master
            string sel = "SELECT name FROM sysdatabases";
            try
            {
                SqlDataAdapter da = new SqlDataAdapter(sel, sCnn);
                da.Fill(dt);
                bases = new string[dt.Rows.Count - 1];
                int k = -1;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string s = dt.Rows[i]["name"].ToString();
                    // Solo asignar las bases que no son del sistema
                    if (Array.IndexOf(basesSys, s) == -1)
                    {
                        k += 1;
                        bases[k] = s;
                    }
                }
                if (k == -1) return null;
                // ReDim Preserve
                {
                    int i1_RPbases = bases.Length;
                    string[] copiaDe_bases = new string[i1_RPbases];
                    Array.Copy(bases, copiaDe_bases, i1_RPbases);
                    bases = new string[(k + 1)];
                    Array.Copy(copiaDe_bases, bases, (k + 1));
                };
                return bases;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Error Al Recuperar Las Bases De Datos...",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        private string ObtenerCadenaDeInsercion()
        {
            try
            {
                //RECORRER DATATABLE PARA MIGRAR LA DATA
                string currentrow = "";
                string sCampos = "";
                for (int i = 0; i < Lst_Campos_Excel.Items.Count; i++)
                {
                    if (i == 0)
                        if (Lst_Campos_Excel.Items[i].ToString().Trim() != TxtCampoIdentity.Text.Trim())
                            sCampos = Lst_Campos_Excel.Items[i].ToString();
                    if (i < Lst_Campos_Excel.Items.Count && i > 0)
                        if (Lst_Campos_Excel.Items[i].ToString().Trim() != TxtCampoIdentity.Text.Trim())
                            if (sCampos.Trim().Length > 0)
                                sCampos += ", " + Lst_Campos_Excel.Items[i].ToString();
                            else
                                sCampos += Lst_Campos_Excel.Items[i].ToString();

                }
                string sComm = "INSERT INTO " + Lst_Tablas.SelectedItem.ToString() + " (" + sCampos + ") VALUES(";
                return sComm;
            }
            catch (Exception jj)
            {
                

            }
            return "-1";
        }

        private string ObtenerTipoCampo(string sNombreCampo)
        {
            try
            {
                //0=Campo; 1=Tipo
                foreach (DataRow row in camposA.Rows)
                {
                    if (row[0].ToString().Trim() == sNombreCampo.Trim())
                    {
                        return row[1].ToString().Trim();
                    }
                    //MessageBox.Show(row[1].ToString());
                }
            }
            catch (Exception jj)
            {
                
            }
            return "vacio";
        }

        private bool ValidarCampos()
        {
            bool bValid = false;
            if (camposA.Rows.Count == Lst_Campos_Excel.Items.Count)
            {
                for (int i = 0; i < Lst_Campos_Excel.Items.Count; i++)
                {
                    if (camposA.Rows[i]["Campo"].ToString() != Lst_Campos_Excel.Items[i].ToString().Trim())
                    {
                        return bValid;
                    }
                }
                bValid = true;
                return bValid;
            }
            return bValid;
        }

        private void cmdImportar_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValidarCampos() == true)
                {
                    string sCnn = "Data Source=" + C_Servidor.Text + ";Initial Catalog=" + C_Bases_De_Datos.Text +
                                  ";Persist Security Info=True;User ID=" + TxtUsuario.Text + ";Password=" +
                                  TxtPassword.Text;
                    SqlConnection sqlCon = new SqlConnection(sCnn);
                    SqlCommand sqlCommand;
                    string sTipo = " ";
                    string sCadenaInstruccion = ObtenerCadenaDeInsercion();
                    string sValorCampo = "";
                    int nIdex_Fila, nIndex_Columna, nFlagX;
                    nIdex_Fila = nIndex_Columna = nFlagX = 0;
                    if (TxtPath.Text.Length > 0)
                    {
                        if (C_Servidor.Text.Length > 0 && C_Bases_De_Datos.Text.Length > 0 &&
                            Lst_Tablas.SelectedItem.ToString().Trim().Length > 0)
                        {
                            sCadenaInstruccion = ObtenerCadenaDeInsercion();
                            Progress_Bar0.Maximum = nTotalReg;
                            Progress_Bar0.Value = 0;
                            foreach (DataRow row in TablaA.Rows)
                            {
                                if (row.RowState != DataRowState.Deleted)
                                {
                                    foreach (DataColumn col in TablaA.Columns)
                                    {
                                        //MessageBox.Show(col.ColumnName+" / "+row[col].ToString());
                                        if (col.ColumnName.Trim() != TxtCampoIdentity.Text.Trim())
                                        {
                                            sTipo = ObtenerTipoCampo(col.ColumnName);
                                            if (row[col].ToString().Trim().Length > 0 && row[col] != null)
                                            {
                                                switch (sTipo)
                                                {
                                                    case "int":
                                                        sValorCampo += " " + Convert.ToInt32(row[col].ToString()) + ", ";
                                                        break;
                                                    case "bit":
                                                        sValorCampo += " " + Convert.ToInt32(row[col].ToString()) + ", ";
                                                        break;
                                                    case "char":
                                                        sValorCampo += "'" + row[col].ToString().Replace("'", " ").TrimEnd() +
                                                                       "', ";
                                                        break;
                                                    case "varchar":
                                                        sValorCampo += "'" + row[col].ToString().Replace("'", " ").TrimEnd() +
                                                                       "', ";
                                                        break;
                                                    case "nvarchar":
                                                        sValorCampo += "'" + row[col].ToString().Replace("'", " ").TrimEnd() +
                                                                       "', ";
                                                        break;
                                                    case "datetime":
                                                        sValorCampo += "'" + Convert.ToDateTime(row[col].ToString()) +
                                                                       "', ";
                                                        break;
                                                    case "text":
                                                        sValorCampo += "'" + row[col].ToString().Replace("'", " ").TrimEnd() +
                                                                       "', ";
                                                        break;
                                                    case "float":
                                                        sValorCampo += " " + Convert.ToDouble(row[col].ToString()) +
                                                                       ", ";
                                                        break;
                                                    case "numeric":
                                                        sValorCampo += " " + Convert.ToDouble(row[col].ToString()) +
                                                                       ", ";
                                                        break;
                                                    case "decimal":
                                                        sValorCampo += " " + Convert.ToDecimal(row[col].ToString()) +
                                                                       ", ";
                                                        break;
                                                    case "image":
                                                        sValorCampo += " null, ";
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                switch (sTipo)
                                                {
                                                    case "int":
                                                        sValorCampo += " " + 0 + ", ";
                                                        break;
                                                    case "bit":
                                                        sValorCampo += " " + 0 + ", ";
                                                        break;
                                                    case "char":
                                                        sValorCampo += "' ', ";
                                                        break;
                                                    case "varchar":
                                                        sValorCampo += "' ', ";
                                                        break;
                                                    case "nvarchar":
                                                        sValorCampo += "' ', ";
                                                        break;
                                                    case "datetime":
                                                        sValorCampo += "' ', ";
                                                        break;
                                                    case "text":
                                                        sValorCampo += "' ', ";
                                                        break;
                                                    case "float":
                                                        sValorCampo += " " + 0.0 + ", ";
                                                        break;
                                                    case "numeric":
                                                        sValorCampo += " " + 0.0 + ", ";
                                                        break;
                                                    case "decimal":
                                                        sValorCampo += " " + 0.0 + ", ";
                                                        break;
                                                    case "image":
                                                        sValorCampo += " null, ";
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    if (sValorCampo.Trim().Length > 0)
                                    {
                                        sValorCampo = sValorCampo.Substring(0, sValorCampo.Length - 2) + ")";
                                        //Insertar en la tabla destino
                                        //MessageBox.Show(sCadenaInstruccion + sValorCampo);
                                        //sValorCampo = "";
                                        sqlCon.Open();
                                        sqlCommand = new SqlCommand(sCadenaInstruccion + sValorCampo, sqlCon);
                                        sqlCommand.ExecuteNonQuery();
                                        sValorCampo = "";
                                        Progress_Bar0.Value++;
                                        sqlCon.Close();
                                    }
                                }
                            }
                            MessageBox.Show("Migracion De Datos Realizada Con Exito...");
                        }
                        else
                        {
                            MessageBox.Show(
                                "Verifique Llenar los campos Servidor, Base de Datos Y Seleccionar Una Tabla...");
                        }
                    }
                    else
                    {
                        MessageBox.Show("No Ha Cargado Ningun Archivo Para Migrar Datos...");
                    }
                }
                else
                    MessageBox.Show("Error: Inconsistencia En El Numero De Campos O Nombres Distintos Entre Campos...");
            }
            catch (Exception j)
            {
                MessageBox.Show("Error Al Migrar Los Datos..." + j.Message);
            }
        }

        private void C_Servidor_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string[] BDD;
                BDD = basesDeDatos(C_Servidor.Text);
                foreach (string s in BDD)
                {
                    C_Bases_De_Datos.Items.Add(s);
                }
            }
            catch(Exception jj)
            {
                MessageBox.Show(jj.ToString());
            }
        }

        //OBTENER CAMPOS DE UNA TABLA
        public void GetColumns(string Servidor, string DataBase, string Tabla)
        {
            // Conexion y query
            string sCnn = "Data Source=" + C_Servidor.Text + ";Initial Catalog=master;Persist Security Info=True;User ID=" + TxtUsuario.Text + ";Password=" + TxtPassword.Text;
            string sel = "Use [" + DataBase + "] Select COLUMN_NAME as Campo, DATA_TYPE as Tipo, CHARACTER_MAXIMUM_LENGTH as Tama�o From INFORMATION_SCHEMA.COLUMNS Where TABLE_NAME ='" + @Tabla + "'";
            //string sel = "Use [" + DataBase + "] Select * From INFORMATION_SCHEMA.COLUMNS Where TABLE_NAME ='" + @Tabla + "'";            
            //COLUMN_NAME,DATA_TYPE,CHARACTER_MAXIMUM_LENGTH
            try
            {
                SqlDataAdapter da = new SqlDataAdapter(sel, sCnn);
                camposA = new DataTable();
                da.Fill(camposA);
                Grd_Estructura.DataSource = "";
                Grd_Estructura.DataSource = camposA;
            }
            catch (Exception ex)
            {
                nFlag = 0;
                MessageBox.Show(ex.Message, "Error al recuperar las bases de la instancia indicada", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //OBTENER TABLAS DE UNA BASE DE DATOS
        public String[] GetTables(string DataBase, string Servidor)
        {
            // Las bases de datos propias de SQL Server
            string[] Tablas;
            DataTable tablas = new DataTable();
            //string sCnn = "Server=" + Servidor + "; database=master; User =" + TxtUsuario.Text + "; Password =" + TxtPassword.Text;
            string sCnn = "Data Source=" + C_Servidor.Text + ";Initial Catalog=master;Persist Security Info=True;User ID=" + TxtUsuario.Text + ";Password=" + TxtPassword.Text;
            string sel = "Use [" + DataBase + "] Select * From INFORMATION_SCHEMA.TABLES Where TABLE_TYPE = 'BASE TABLE'";
            try
            {
                SqlDataAdapter da = new SqlDataAdapter(sel, sCnn);
                da.Fill(tablas);
                Tablas = new string[tablas.Rows.Count];
                //int k = -1;
                for (int i = 0; i < tablas.Rows.Count; i++)
                {
                    string s = tablas.Rows[i]["TABLE_NAME"].ToString();
                    Tablas[i] = s;
                }
                nFlag = 1;
                return Tablas;
            }
            catch (Exception ex)
            {
                nFlag = 0;
                MessageBox.Show(ex.Message, "Error Al Recuperar Las Tablas De La Base De Datos...", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        private void ObtenerCampoAutoIncremental()
        {
            try
            {
                DataTable Temp = new DataTable();
                string sel = "";
                string sCnn = sCnn = "Data Source=" + C_Servidor.Text + ";Initial Catalog=" + C_Bases_De_Datos.Text + ";Persist Security Info=True;User ID=" + TxtUsuario.Text + ";Password=" + TxtPassword.Text;
                SqlDataAdapter da;
                string sNombreCampo = "";
                TxtCampoIdentity.Text = "";
                foreach (DataRow row in camposA.Rows)
                {
                    //MessageBox.Show(row[nIdex_Fila].ToString());
                    sNombreCampo = row[0].ToString();
                    sel = "SELECT COLUMNPROPERTY(OBJECT_ID('" + Lst_Tablas.SelectedItem.ToString() + "'),'" + sNombreCampo + "','IsIdentity') as Resultado";
                    try
                    {
                        da = new SqlDataAdapter(sel, sCnn);
                        da.Fill(Temp);
                        if (Convert.ToInt32(Temp.Rows[0]["Resultado"].ToString()) == 1)
                        {
                            TxtCampoIdentity.Text = sNombreCampo;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error al recuperar las bases de la instancia indicada", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception jj)
            {
                
            }
        }

        private void C_Bases_De_Datos_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string[] sTablas;
                sTablas = GetTables(C_Bases_De_Datos.Text, C_Servidor.Text);
                Lst_Tablas.Items.Clear();
                foreach (string s in sTablas)
                {
                    Lst_Tablas.Items.Add(s);
                }
            }
            catch(Exception jj)
            {
                MessageBox.Show(jj.ToString());
            }
        }

        private void Lst_Tablas_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string sTabla = Lst_Tablas.SelectedItem.ToString();
                GetColumns(C_Servidor.Text, C_Bases_De_Datos.Text, sTabla);
                ObtenerCampoAutoIncremental();
            }
            catch(Exception jj)
            {
                MessageBox.Show(jj.ToString());
            }
        }

        private void C_Hoja_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                CargarHojaDeExcel(TxtPath.Text.Trim());
                TxtTotalRegistros.Text = (nTotalReg - 1).ToString();
            }
            catch (Exception jj)
            {
                
            }
        }

        private void tabControl0_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tabControl0.SelectedIndex == 2)
                {
                    if (C_Servidor.Text.Length > 0 && C_Bases_De_Datos.Text.Length > 0)
                    {
                        TxtServidorActual.Text = C_Servidor.Text.Trim();
                        TxtBaseDeDatos.Text = C_Bases_De_Datos.Text.Trim();
                    }
                }
            }
            catch (Exception jj)
            {
                
            }
        }

        private bool EsEntero(string sCadena)
        {
            bool bResp = false;
            try
            {
                int i = 0;
                bResp = int.TryParse(sCadena, out i);
            }
            catch (Exception jj)
            {
                
            }
            return bResp;
        }

        //DETERMINA SI EL STRING PUEDE CONVERTIRSE EN NUMERO O ES CADENA
        private bool EsDecimal(string sCadena)
        {
            bool bResp = false;
            try
            {
                double isItNumeric;
                bResp = Double.TryParse(Convert.ToString(sCadena), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out isItNumeric);
            }
            catch (Exception jj)
            {
                
            }
            return bResp;
        }

        //DETERMINA SI EL STRING PUEDE CONVERTIRSE EN FECHA
        private bool EsFecha(string sCadena)
        {
            bool bResp = false;
            try
            {
                DateTime dtFecha;
                CultureInfo culture;
                DateTimeStyles styles;
                culture = CultureInfo.CreateSpecificCulture("en-US");
                styles = DateTimeStyles.None;
                bResp = DateTime.TryParse(sCadena, culture, styles, out dtFecha);
            }
            catch (Exception jj)
            {

            }
            return bResp;
        }

        private void cmdAnalizis_Click(object sender, EventArgs e)
        {
            try
            {
                int nLLaveP = 0;
                string sNombreCampo = "";
                string sTipo = "";
                int nTama�o = 0;
                int nEsNulo = 0;
                bool Es_Numero = false;
                bool Es_Entero = false;
                bool Es_Decimal = false;
                bool Es_Fecha = false;
                int nIdiceFila = 0;
                int nFlagNum = 0;
                int nFlagEntero = 0;
                int nFlagFecha = 0;
                if (TxtPath.Text.Length > 0 && TablaA.Rows.Count > 0 && Lst_Campos_Excel.Items.Count > 0)
                {
                    //Preparar Grid Con El Numero De Lineas Adecuado
                    Grd_SugerenciaDeEstructura.RowCount = Lst_Campos_Excel.Items.Count;
                    //INICIAR ANALIZIS
                    foreach (DataColumn myProperty in TablaA.Columns)
                    {
                        nTama�o = nFlagNum = nFlagEntero = 0;
                        Es_Numero = Es_Entero = Es_Decimal = Es_Fecha = false;

                        foreach (DataRow row in TablaA.Rows)
                        {
                            if (row.RowState != DataRowState.Deleted)
                            {
                                //MessageBox.Show(row[myProperty.ColumnName].ToString());
                                //OBTENER LONGITUD MAXIMA
                                if (nTama�o < row[myProperty.ColumnName].ToString().Trim().Length)
                                    nTama�o = row[myProperty.ColumnName].ToString().Trim().Length;
                                Es_Decimal = EsDecimal(row[myProperty.ColumnName].ToString().Trim());
                                if (Es_Decimal == true)
                                {
                                    if (EsEntero(row[myProperty.ColumnName].ToString().Trim()) == true)
                                        nFlagEntero++;
                                    nFlagNum++;
                                }
                                Es_Fecha = EsFecha(row[myProperty.ColumnName].ToString().Trim());
                                if (Es_Fecha == true)
                                {
                                    nFlagFecha++;
                                }
                            }
                        }
                        //Verificar Si Es Numero 
                        if (nFlagNum == nTotalReg - 1) //Menos 1 por el header de cada columna
                            Es_Numero = true;
                        sNombreCampo = myProperty.ColumnName;
                        if (Es_Numero != true)
                        {
                            if (nTama�o >= 0 && nTama�o <= 4000)
                            {
                                //Establecer Valor Por Defecto Si El Tama�o Es 0
                                if (nTama�o == 0)
                                    nTama�o = 10;
                                sTipo = "VarChar(" + nTama�o + ")";
                            }
                            if (nTama�o > 4000)
                                sTipo = "Text";
                        }
                        else
                        {
                            if (nFlagEntero == nTotalReg - 1)
                                sTipo = "int";
                            else
                                sTipo = "float";
                        }
                        if (Es_Fecha == true)
                        {
                            if (nFlagFecha == nTotalReg - 1)
                                sTipo = "DateTime";
                        }
                        //Recorrer Gird
                        int i = Grd_SugerenciaDeEstructura.Columns.Count;
                        int j = Grd_SugerenciaDeEstructura.Rows.Count;
                        //Setear Datos En El Grid
                        //Llave Primaria
                        Grd_SugerenciaDeEstructura[0, nIdiceFila].Value = 0; //nIdiceFila+1, 0
                        //Nombre Campo
                        Grd_SugerenciaDeEstructura[1, nIdiceFila].Value = Lst_Campos_Excel.Items[nIdiceFila].ToString().Trim();
                        //Tipo
                        Grd_SugerenciaDeEstructura[2, nIdiceFila].Value = sTipo;
                        //Es Nulo
                        Grd_SugerenciaDeEstructura[3, nIdiceFila].Value = 0;
                        nIdiceFila++;
                    }
                }
                else
                {
                    MessageBox.Show("Necesita Cargar Una Hoja De Calculo Antes...");
                }
            }
            catch (Exception jj)
            {                
                
            }
        }

        private bool ExisteTabla()
        {
            DataTable tablas = new DataTable();
            string sCnn = "Data Source=" + C_Servidor.Text + ";Initial Catalog=master;Persist Security Info=True;User ID=" + TxtUsuario.Text + ";Password=" + TxtPassword.Text;
            string sel = "Use [" + C_Bases_De_Datos.Text + "] Select * From INFORMATION_SCHEMA.TABLES Where TABLE_TYPE = 'BASE TABLE'";
            try
            {
                SqlDataAdapter da = new SqlDataAdapter(sel, sCnn);
                da.Fill(tablas);
                //int k = -1;
                for (int i = 0; i < tablas.Rows.Count; i++)
                {
                    string s = tablas.Rows[i]["TABLE_NAME"].ToString().Trim();
                    if(s==TxtNombreTabla.Text.Trim())
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                nFlag = 0;
                MessageBox.Show(ex.Message, "Error al recuperar las Tablas En La Instancia Indicada", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        private bool ValidarNombres()
        {
            bool bResp = false;
            try
            {
                int nFlagValid = 0;

                if (TxtServidorActual.Text.Trim().Length == 0 || TxtServidorActual.Text == null)
                    nFlagValid++;

                if (TxtBaseDeDatos.Text.Trim().Length == 0 || TxtBaseDeDatos.Text == null)
                    nFlagValid++;

                if (TxtNombreTabla.Text.Trim().Length == 0 || TxtNombreTabla.Text == null)
                    nFlagValid++;

                if (nFlagValid == 0)
                {
                    bResp = true;
                }
            }
            catch (Exception jj)
            {
                
            }
            return bResp;
        }

        private void cmd_CrearEstructura_Click(object sender, EventArgs e)
        {

            // Conexion y query
            string sCnn = "Data Source=" + C_Servidor.Text + ";Initial Catalog="+C_Bases_De_Datos.Text+";Persist Security Info=True;User ID=" + TxtUsuario.Text + ";Password=" + TxtPassword.Text;
            string sInstruccion = "Use [" + C_Bases_De_Datos.Text + "] Create Table "+TxtNombreTabla.Text.Trim()+" (";
            string sCampos = "";
            string sCampo = "";
            string sTipo = "";
            bool bEsLLavePrimaria = false;
            bool bEsAutoIncremental = false;
            int nLLaves = 0;
            int nFlagAutoIncremental = 0; //Si Existe un autoincremental cuyo tipo es distinto de int debe dar error
            //COLUMN_NAME,DATA_TYPE,CHARACTER_MAXIMUM_LENGTH
            if (ExisteTabla() != true)
            {
                if (ValidarNombres() == true)
                {
                    try
                    {
                        //Obtener Cadena De Campos Para Creacion De Tabla
                        for (int i = 0; i < Grd_SugerenciaDeEstructura.RowCount; i++) // filas
                        {
                            for (int j = 0; j < Grd_SugerenciaDeEstructura.ColumnCount; j++) //columnas
                            {
                                switch (j)
                                {
                                        //Ver Si Es Llave Primara
                                    case 0:
                                        if (Grd_SugerenciaDeEstructura[j, i].Value.ToString() == "1")
                                        {
                                            bEsLLavePrimaria = true;
                                            nLLaves++;
                                        }
                                        break;
                                        //Obtener Nombre Del Campo
                                    case 1:
                                        sCampo = Grd_SugerenciaDeEstructura[j, i].Value.ToString();
                                        break;
                                        //Obtener Tipo
                                    case 2:
                                        sTipo = Grd_SugerenciaDeEstructura[j, i].Value.ToString();
                                        break;
                                        //Ver Si AEl Valores Es AutoIncremental
                                    case 3:
                                        if (Grd_SugerenciaDeEstructura[j, i].Value.ToString() == "1")
                                        {
                                            bEsAutoIncremental = true;
                                        }
                                        break;
                                }
                                if (bEsAutoIncremental==true && sTipo.Trim()!="int")
                                {
                                    nFlagAutoIncremental++;
                                }
                            }
                            if (i >= 0 && i < Grd_SugerenciaDeEstructura.RowCount - 1)
                            {
                                if (bEsAutoIncremental == true && bEsLLavePrimaria == false)
                                    sCampos = sCampo + " " + sTipo + " IDENTITY (1,1), ";
                                        //IDENTITY (IniciaDesde, SeIncrementaEn)

                                if (bEsAutoIncremental == true && bEsLLavePrimaria == true)
                                    sCampos = sCampo + " " + sTipo + " IDENTITY (1,1) PRIMARY KEY, ";

                                if (bEsLLavePrimaria == true && bEsAutoIncremental == false)
                                    sCampos = sCampo + " " + sTipo + " PRIMARY KEY" + ", ";

                                if (bEsLLavePrimaria == false && bEsAutoIncremental == false)
                                    sCampos = sCampo + " " + sTipo + ", ";
                            }
                            else
                            {
                                if (bEsAutoIncremental == true && bEsLLavePrimaria == false)
                                    sCampos = sCampo + " " + sTipo + " IDENTITY (1,1) )";

                                if (bEsAutoIncremental == false && bEsLLavePrimaria == false)
                                    sCampos = sCampo + " " + sTipo + " IDENTITY (1,1) PRIMARY KEY )";

                                if (bEsLLavePrimaria == true && bEsAutoIncremental == false)
                                    sCampos = sCampo + " " + sTipo + " PRIMARY KEY" + ")";

                                if (bEsLLavePrimaria == false && bEsAutoIncremental == false)
                                    sCampos = sCampo + " " + sTipo + ")";
                            }
                            bEsLLavePrimaria = false;
                            bEsAutoIncremental = false;
                            sInstruccion += sCampos;
                            sCampo = "";
                        }
                        if (nFlagAutoIncremental == 0)
                        {
                            if (nLLaves >= 0 && nLLaves <= 1)
                            {
                                //Establecer Conexion
                                SqlConnection cn = new SqlConnection();
                                cn.ConnectionString = sCnn;
                                cn.Open();
                                //Setear Comando SQL
                                SqlCommand cmd = new SqlCommand(sInstruccion, cn);
                                //Crear Tabla
                                cmd.ExecuteNonQuery();
                                C_Bases_De_Datos_SelectedIndexChanged(sender, e);
                                MessageBox.Show("La Tabla: " + TxtNombreTabla.Text.Trim() + " se ha creado con exito...");
                            }
                            else
                            {
                                MessageBox.Show("Solo Debe Haber 1 Llave Primaria...");
                            }
                        }
                        else
                        {
                            MessageBox.Show("El Campo AutoIncremental Solo Es Soportado En Esta Version Para Campos De Tipo Int...");
                        }
                    }
                    catch (Exception ex)
                    {
                        nFlag = 0;
                        MessageBox.Show("Error:" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Error: Debe especificar El Servidor, La Base De Datos Y Un Nombre De Tabla");
                }
            }
            else
            {
                MessageBox.Show("Error: Ya Existe Una Tabla Con El Nombre:"+TxtNombreTabla.Text);
            }
        }

        

        private void C_Servidor_Leave(object sender, EventArgs e)
        {
            try
            {
                string[] BDD;
                BDD = basesDeDatos(C_Servidor.Text);
                foreach (string s in BDD)
                {
                    C_Bases_De_Datos.Items.Add(s);
                }
            }
            catch (Exception jj)
            {
                MessageBox.Show(jj.ToString());
            }
        }
     //Fin De Clase
    }
//Fin De Nombre
}
