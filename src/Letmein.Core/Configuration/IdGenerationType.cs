namespace Letmein.Core.Configuration
{
	public enum IdGenerationType
	{
		Default = RandomWithProunceable,

		RandomWithProunceable = 1,
		Prounceable = 2,
		ShortPronounceable = 3,
		ShortMixedCase = 4,
		ShortCode = 5
	}
}