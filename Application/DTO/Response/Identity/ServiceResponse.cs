namespace Application.DTO.Response.Identity;

public class ServiceResponse(bool Flag, bool Message)
{
	public bool Flag { get; set; }
	public  bool Message { get; set; }
}