using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls.WebParts;

namespace Propiedades
{
    public class CuentaBancaria
    {
        public static string Banco { get; set; }
        public string NoCuenta { get; set; }
        public string Usuario { get; set; }

        private decimal _saldo;

        public decimal Saldo
        {
            get { return _saldo; }
            set
            {
                if (value < 0)
                {
                    _saldo = 0;
                }
                else
                {
                    _saldo = value;
                }

            }
        }

        #region constructores
        public CuentaBancaria(string noCuenta)
        {
            NoCuenta = noCuenta;
        }

        public CuentaBancaria(string noCuenta, string usuario)
            : this(noCuenta)
        {
            Usuario = usuario;
        }

        public CuentaBancaria(string noCuenta, string usuario, decimal saldo)
            : this(noCuenta, usuario)
        {
            Saldo = saldo;
        }

        public CuentaBancaria() { }
        #endregion


        public void Depositar(decimal cantidad) {

            Saldo += cantidad;
        
        }

        public void Retirar(decimal cantidad)
        {
            Saldo -= cantidad;
        }


        public override string ToString()
        {
            return string.Format("Banco: {0}, NoCuenta: {1}, Usuario: {2}, Saldo ${3}", Banco, NoCuenta, Usuario, Saldo);
          
        }

        public static void AsignarBanco(string Banco) //METODO STATICO
        {
            Console.WriteLine("Asignando nuevo banco....");
            Banco = Banco;
        
        }

    }
}