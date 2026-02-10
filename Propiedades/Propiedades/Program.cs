using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Propiedades
{
    /// <summary>
    /// Propiedades
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {


            //PARA ACCEDER A LOS MIEMBROS ESTATICOS SE USA SOLO EL NOMBRE DE LA CLASE
            CuentaBancaria.Banco = "Mi banco";
            CuentaBancaria cuenta1 = new CuentaBancaria("001", "Gregori Jimenez", 100);
            CuentaBancaria cuenta2 = new CuentaBancaria("002", "Rumys Campos", 200);

            //LLAMANDO LOS METODOS DEPOSITAR Y RETIRAR 

           
            Console.WriteLine(" Deposita $70 en cuenta1");
            cuenta1.Depositar(70);

            Console.WriteLine("Retirae $20 en cuenta2");
            cuenta2.Retirar(20);

            Console.WriteLine("Saldo cuenta1: " + cuenta1.Saldo);
            Console.WriteLine("Saldo cuenta2: " + cuenta2.Saldo);

            Console.WriteLine(cuenta1);
            Console.WriteLine(cuenta2);


            Console.WriteLine("Cambiando el banco");

            CuentaBancaria.Banco = "Nuevo Banco";


            Console.WriteLine(cuenta1);
            Console.WriteLine(cuenta2);


        }
    }
}

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
    }
}
