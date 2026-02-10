using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PSC10
{
    public partial class frmRecibos: Form
    {
        public frmRecibos()
        {
            InitializeComponent();
            EstiloDataGridView();
        }

        string oldValue;
        string newValor;
        string newValue;
        double nTotal;

        private void frmRecibos_Load(object sender, EventArgs e)
        {
            this.Text = "Recibos";
            this.KeyPreview = true;

            lblFecha.Text = DateTime.Now.ToString("yyyyMMdd");
            string _recibo = Busco.BuscaUltimoNumero("3");  // buscamos el ultimo recibo a realizar

            lblRecibo.Text = Convert.ToInt32(_recibo).ToString("D10"); // ahora tiene 10 posiciones
        }

        private void frmRecibos_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        // --------------------------------------------------------
        // TEXTBOX
        // --------------------------------------------------------
        private void txtCliente_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == (int)Keys.Enter)
            {
                e.Handled = true;
                if (txtCliente.Text.Trim() != string.Empty)
                {
                    txtValor.Focus();
                }
            }
        }

        private void txtCliente_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F4)
            {
                btnVENCTE.PerformClick();
            }
        }

        private void txtCliente_Leave(object sender, EventArgs e)
        {
            if (txtCliente.Text.Trim() != string.Empty)
            {
                lblNombre.Text = Busco.BuscaNombreCliente(txtCliente.Text);

                BuscarMovimientosCliente(txtCliente.Text);
            }
        }

        private void txtValor_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == (int)Keys.Enter)
            {
                e.Handled = true;
                if (txtValor.Text.Trim() != string.Empty)
                {
                    dgv.Focus();  // mueve el focus hacia adentro de la DatagridView
                }
            }
        }


        private void txtValor_Leave(object sender, EventArgs e)
        {
            string texto = txtValor.Text.Trim();

            if (string.IsNullOrEmpty(texto))
                return;

            string textoNormalizado = texto.Replace(',', '.');

            if (!decimal.TryParse(textoNormalizado, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valor))
            {
                MessageBox.Show("Ingrese una cantidad válida.", "ITLA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtValor.Focus();
                return;
            }

            txtValor.Text = valor.ToString("N2");
        }

        // ---------------------------------------------------------------------
        // MANEJO DE LA GRILLA
        // ---------------------------------------------------------------------
        private void dgv_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex != 4)
            {
                return;
            }
            // toma el valor de la celda
            oldValue = (string)dgv[e.ColumnIndex, e.RowIndex].Value;
        }

        private void dgv_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 4)
            {
                return;
            }
            // actualiza el nuevo valor
            newValor = (string)dgv[e.ColumnIndex, e.RowIndex].Value;

            if (newValor != string.Empty)
            {
                newValue = (string)dgv[4, e.RowIndex].Value;
                ActualizaTotalRecibo();
            }
            else
            {
                dgv[e.ColumnIndex, e.RowIndex].Value = oldValue;  // si presionaste ESC coloca el valor anterior
            }
        }


        // --------------------------------------------------------
        // BOTONES
        // --------------------------------------------------------
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (lblTotal.Text.Trim() == txtValor.Text.Trim())
            {
                InsertaMvtoCliente();

                // -----------------------
                // VISTA PREVIA DEL RECIBO
                // -----------------------
                BorrarTablasReporte();

                LlenarReciboHeaderReporte();
                LlenarReciboDetailsReporte();

                //frmRPT000 frm = new frmRPT000(@"C:\ITLA\Reportes\rptRecibos.rpt", "Recibo");
                //frm.Show();

                // -----------------------
                btnLimpiar.PerformClick();
            }
            else
            {
                MessageBox.Show("Tenemos diferencia sobre el monto recibo y el pagado", "ITLA", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            txtCliente.Focus();
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnVENCTE_Click(object sender, EventArgs e)
        {
            frmVENCTE frm = new frmVENCTE();
            frm.ShowDialog();

            if (frm.EData == true)
            {
                txtCliente.Text = frm.var1;
                lblNombre.Text = frm.var2;

                BuscarMovimientosCliente(txtCliente.Text);
            }
        }



        // --------------------------------------------------------
        // METODOS
        // --------------------------------------------------------
        private void LimpiarFormulario()
        {
            this.dgv.Rows.Clear();
            this.dgv.Refresh();

            string _recibo = Busco.BuscaUltimoNumero("2");

            lblRecibo.Text = Convert.ToInt32(_recibo).ToString("D10"); // ahora tiene 8 posiciones

            txtCliente.Clear();
            lblNombre.Text = "";
            lblTotal.Text = "";

            lblFecha.Text = DateTime.Now.ToString("yyyyMMdd");
        }

        private void BuscarMovimientosCliente(string nmrCliente)
        {
            string sQuery = @"SELECT IDCLIENTE, FECHA, TIPDOC, DOCUMENTO, APLICADO, ORIGEN, MONTO, BCEPENDIENTE 
                                FROM MVTOCTE 
                               WHERE IDCLIENTE = @A1
                                 AND ORIGEN    = @A2 
                                 AND ACTIVO    = 1
                            ORDER BY FECHA, DOCUMENTO ASC";

            SqlConnection cxn = new SqlConnection(cnn.db);  cxn.Open();
            SqlCommand cmd = new SqlCommand(sQuery, cxn);
            cmd.Parameters.AddWithValue("@A1", nmrCliente);
            cmd.Parameters.AddWithValue("@A2", "1");
            SqlDataReader rsd = cmd.ExecuteReader();

            while (rsd.Read())
            {
                dgv.Rows.Add();
                int xRows = dgv.Rows.Count - 1;

                dgv[0, xRows].Value = rsd["TIPDOC"].ToString();
                dgv[1, xRows].Value = rsd["DOCUMENTO"].ToString();
                dgv[2, xRows].Value = rsd["FECHA"].ToString();
                dgv[3, xRows].Value = rsd["BCEPENDIENTE"].ToString();
                dgv[4, xRows].Value = "0";
            }
        }

        private void ActualizaTotalRecibo()
        {
            lblTotal.Text = "";
            nTotal = 0;

            foreach (DataGridViewRow row in dgv.Rows)
            {
                double abono = Convert.ToDouble( row.Cells[4].Value.ToString());
                nTotal = nTotal + abono;

                lblTotal.Text = nTotal.ToString();
            }
        }


        private void BorrarTablasReporte()
        {
            string _rxs = "S";

            if (_rxs == "S")
            {
                string stQuery = "DELETE FROM ReciboHeader";
                OleDbConnection cxn = new OleDbConnection(cnn.ac); cxn.Open();
                OleDbCommand cmd = new OleDbCommand(stQuery, cxn);

                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            if (_rxs == "S")
            {
                string stQuery = "DELETE FROM MVTOcte";
                OleDbConnection cxn = new OleDbConnection(cnn.ac); cxn.Open();
                OleDbCommand cmd = new OleDbCommand(stQuery, cxn);

                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }

        private void LlenarReciboHeaderReporte()
        {
            string stQuery = "INSERT INTO RECIBOHEADER ( RECIBO, CLIENTE, NOMBRE, FECHA, MONTO ) VALUES (@A1, @A2, @A3, @A4, @A5 )";
            OleDbConnection cxn = new OleDbConnection(cnn.ac); cxn.Open();
            OleDbCommand cmd = new OleDbCommand(stQuery, cxn);

            cmd.Parameters.AddWithValue("@A1", lblRecibo.Text);
            cmd.Parameters.AddWithValue("@A2", txtCliente.Text);
            cmd.Parameters.AddWithValue("@A3", lblNombre.Text);   
            cmd.Parameters.AddWithValue("@A4", lblFecha.Text);
            cmd.Parameters.AddWithValue("@A5", txtValor.Text);    

            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        private void LlenarReciboDetailsReporte()
        {
            string stQuery = "INSERT INTO MVTOCTE (IDCLIENTE, FECHA, DOCUMENTO, APLICADO, ORIGEN, MONTO, BCEPENDIENTE, TIPDOC ) " +
                             "VALUES ( @A1, @A2, @A3, @A4, @A5, @A6, @A7, @A8 )";

            OleDbConnection cxn = new OleDbConnection(cnn.ac); cxn.Open();

            foreach (DataGridViewRow row in dgv.Rows)
            {
                string nmDoc = row.Cells[0].Value.ToString() ?? "";
                string nmPag = row.Cells[3].Value.ToString() ?? "";

                OleDbCommand cmd = new OleDbCommand(stQuery, cxn);
                cmd.Parameters.AddWithValue("@A1", txtCliente.Text);
                cmd.Parameters.AddWithValue("@A2", lblFecha.Text);
                cmd.Parameters.AddWithValue("@A3", nmDoc);    // AQUI SE COLOCA EL DOCUMENTO ORIGEN DEBITO
                cmd.Parameters.AddWithValue("@A4", lblRecibo.Text);
                cmd.Parameters.AddWithValue("@A5", "C");
                cmd.Parameters.AddWithValue("@A6", nmPag);    // PAGO REALIZADO A DOCUMENTO DE ORIGEN DEBITO
                cmd.Parameters.AddWithValue("@A7", "0");
                cmd.Parameters.AddWithValue("@A8", "RC");

                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }

        private void InsertaMvtoCliente()
        {
            string stQuery = "INSERT INTO MVTOCTE (IDCLIENTE, FECHA, DOCUMENTO, APLICADO, ORIGEN, MONTO, BCEPENDIENTE, TIPDOC ) " +
                             "VALUES ( @A1, @A2, @A3, @A4, @A5, @A6, @A7, @A8 )";

            SqlConnection cxn = new SqlConnection(cnn.db); cxn.Open();

            foreach (DataGridViewRow row in dgv.Rows)
            {
                string nmDoc = row.Cells[0].Value.ToString() ?? "";
                string nmPag = row.Cells[3].Value.ToString() ?? "";

                SqlCommand cmd = new SqlCommand(stQuery, cxn);
                cmd.Parameters.AddWithValue("@A1", txtCliente.Text);
                cmd.Parameters.AddWithValue("@A2", lblFecha.Text);
                cmd.Parameters.AddWithValue("@A3", nmDoc);    // AQUI SE COLOCA EL DOCUMENTO ORIGEN DEBITO
                cmd.Parameters.AddWithValue("@A4", lblRecibo.Text);
                cmd.Parameters.AddWithValue("@A5", "2");
                cmd.Parameters.AddWithValue("@A6", nmPag);    // PAGO REALIZADO A DOCUMENTO DE ORIGEN DEBITO
                cmd.Parameters.AddWithValue("@A7", "0");
                cmd.Parameters.AddWithValue("@A8", "RC");

                cmd.ExecuteNonQuery();
                cmd.Dispose();

                ActualizaBalanceDocumento(txtCliente.Text, nmDoc, nmPag);
                CambiarEstatusDocumento(txtCliente.Text, nmDoc);
            }

            ActualizaBalanceCliente(txtCliente.Text, lblTotal.Text);
            ActualizaUltimoSecuenciaRecibo(lblRecibo.Text);
        }

        private void ActualizaUltimoSecuenciaRecibo(string numSecuencia)
        {
            string sQuery = "UPDATE SECUENCIAS " +
                            "   SET CONTADOR ='" + numSecuencia +
                            "' FROM SECUENCIAS " +
                            " WHERE ID ='2'";

            SqlConnection cxn = new SqlConnection(cnn.db); cxn.Open();
            SqlCommand cmd = new SqlCommand(sQuery, cxn);
            cmd.ExecuteNonQuery();
            cxn.Close();
        }

        private void CambiarEstatusDocumento(string nmrCliente, string nmrDocu)
        {
            string sQuery = "SELECT BCEPENDIENTE " +
                            "  FROM MVTOCTE " +
                            " WHERE IDCLIENTE ='" + nmrCliente +
                            "'  AND DOCUMENTO ='" + nmrDocu +
                            "'  AND ORIGEN    ='1'";
            SqlConnection cxn = new SqlConnection(cnn.db); cxn.Open();
            SqlCommand cmd = new SqlCommand(sQuery, cxn);
            SqlDataReader rsd = cmd.ExecuteReader();

            if (rsd.Read())
            {
                double bce = Convert.ToDouble(rsd["BCEPENDIENTE"].ToString());

                if (bce == 0)
                {
                    string sQueri = "UPDATE MVTOCTE " +
                                    "   SET ACTIVO = 0 " +
                                    "  FROM MVTOCTE " +
                                    " WHERE IDCLIENTE = '" + nmrCliente +
                                    "'  AND DOCUMENTO = '" + nmrDocu +
                                    "'  AND ORIGEN    = '1'";
                    SqlConnection sxn = new SqlConnection(cnn.db); sxn.Open();
                    SqlCommand smd = new SqlCommand(sQueri, sxn);
                    smd.ExecuteNonQuery();
                }
            }
        }


        private void ActualizaBalanceCliente(string numCliente, string TotalReciboCliente)
        {
            string sQuery = "UPDATE CLIENTES " +
                            "   SET BALANCEPENDIENTE = BALANCEPENDIENTE - " + TotalReciboCliente +
                            "  FROM CLIENTES " +
                            " WHERE IDCLIENTE ='" + numCliente + "'";

            SqlConnection cxn = new SqlConnection(cnn.db); cxn.Open();
            SqlCommand cmd = new SqlCommand(sQuery, cxn);
            cmd.ExecuteNonQuery();
            cxn.Close();
        }

        private void ActualizaBalanceDocumento(string nmrCliente, string nmrDocu, string nmrPago)
        {
            // vamos actualizar al documento de origen debito, al que se le esta haciendo el pago

            string sQuery = "UPDATE MVTOCTE " +
                            "   SET BCEPENDIENTE = BCEPENDIENTE - " + nmrPago +
                            "  FROM MVTOCTE " +
                            " WHERE IDCLIENTE = '" + nmrCliente +
                            "'  AND DOCUMENTO = '" + nmrDocu +
                            "'  AND ORIGEN    = '1'";

            SqlConnection cxn = new SqlConnection(cnn.db); cxn.Open();
            SqlCommand cmd = new SqlCommand(sQuery, cxn);
            cmd.ExecuteNonQuery();
        }

        private void EstiloDataGridView()
        {
            this.dgv.EnableHeadersVisualStyles = false;
            this.dgv.AllowUserToAddRows = false;
            this.dgv.AllowUserToDeleteRows = false;
            this.dgv.ColumnHeadersVisible = true;
            this.dgv.RowHeadersVisible = false;

            this.dgv.Columns.Add("Col00", "Tipo");
            this.dgv.Columns.Add("Col01", "Documento");
            this.dgv.Columns.Add("Col02", "Fecha");
            this.dgv.Columns.Add("Col03", "Bce Pendiente");
            this.dgv.Columns.Add("Col04", "Valor a Pagar");

            DataGridViewColumn
            column = dgv.Columns[00]; column.Width = 100;
            column = dgv.Columns[01]; column.Width = 150;
            column = dgv.Columns[02]; column.Width = 150;
            column = dgv.Columns[03]; column.Width = 150;
            column = dgv.Columns[04]; column.Width = 200;

            this.dgv.BorderStyle = BorderStyle.None;
            this.dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(238, 239, 249);
            this.dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgv.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            this.dgv.DefaultCellStyle.SelectionForeColor = Color.WhiteSmoke;
            this.dgv.BackgroundColor = Color.LightGray;

            this.dgv.EnableHeadersVisualStyles = false;
            this.dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            this.dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 6, 0, 6);
            this.dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.CornflowerBlue;
            this.dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }

    }
}
