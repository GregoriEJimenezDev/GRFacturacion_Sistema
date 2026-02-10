<%@ Page Language="C#" %>
<!DOCTYPE html>
<html>
<head>
    <title>Propiedades</title>
</head>
<body>
    <%
        CuentaBancaria.Banco = "Mi banco";
        CuentaBancaria cuenta1 = new CuentaBancaria("001", "Gregori Jimenez", 100);
        CuentaBancaria cuenta2 = new CuentaBancaria("002", "Rumys Campos", 200);

        Response.Write("Deposita $70 en cuenta1<br>");
        cuenta1.Depositar(70);

        Response.Write("Retira $20 en cuenta2<br>");
        cuenta2.Retirar(20);

        Response.Write("Saldo cuenta1: " + cuenta1.Saldo + "<br>");
        Response.Write("Saldo cuenta2: " + cuenta2.Saldo + "<br>");

        Response.Write(cuenta1.ToString() + "<br>");
        Response.Write(cuenta2.ToString() + "<br>");

        CuentaBancaria.Banco = "Nuevo Banco";
        Response.Write("Después de cambiar el banco:<br>");
        Response.Write(cuenta1.ToString() + "<br>");
        Response.Write(cuenta2.ToString() + "<br>");
    %>
</body>
</html>