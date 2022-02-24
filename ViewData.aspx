<%@ Page Language="C#" CodeBehind="View data.aspx.cs" Inherits="MyMovieSite.View_data" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Title</title>
    <link rel="stylesheet" runat="server" media="screen" href="/Content/main.css" />
</head>
<body>
<form id="HtmlForm" runat="server">
    <div class="Left">
        <div style="display: inline-grid">
            <div style="height: 50px"></div>
            <div>
                <asp:Label ID="Label1" runat="server" Text="Movie Name:"/>
                <asp:TextBox placeholder="Movie name" ID="TextBox1" runat="server" />
            </div>
            <div>
                <asp:Label text="Genre:" id="Label2" runat="server"/>
                <asp:TextBox placeholder="Rating 0-5" runat="server"  id="TextBox2" />
            </div>
            <div>
                <asp:Label text="Rating:" runat="server" id="Label3"/>
                <asp:TextBox placeholder="Movie Description" runat="server" id="TextBox3" />
            </div>
            <asp:Button Text="Submit" OnCommand="Button1_Click" runat="server"/>
        </div>
    </div>
    <div class="Right">
        
    </div>
</form>
</body>
</html>