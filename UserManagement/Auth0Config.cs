namespace UserManagement;

class Auth0Config
{
    public string Domain { get; set; } = "";
    public string Client_Id { get; set; } = "";
    public string Client_Secret { get; set; } = "";
    public string Audience { get; set; } = "";
}