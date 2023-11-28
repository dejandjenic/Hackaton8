namespace API.Repositories;

public record ChatHistory
{
	/// <summary>
	/// Unique identifier, partition key
	/// </summary>
	public string Id { get; set; }
	public string Type { get; set; }
	public string UserId { get; set; }
	public bool FromUser { get; set; }
	public string Text { get; set; }
	public DateTime Date { get; set; }
	public string FromWho { get; set; } //ChatRole.Assistant or ChatRole.User or ChatRole.System

	public ChatHistory(string userId, bool fromUser, string text, DateTime date, string fromWho)
	{
		Id = Guid.NewGuid().ToString();
		UserId = userId;
		FromUser = fromUser;
		Text = text;
		Date = date;
		FromWho = fromWho;
		Type = nameof(ChatHistory);
	}
}

