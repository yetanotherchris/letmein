namespace Letmein.Core.Configuration
{
	public enum IdGenerationType
	{
		Default = RandomWithPronounceable,

		RandomWithPronounceable = 1,
		Pronounceable = 2,
		ShortPronounceable = 3,
		ShortMixedCase = 4,
		ShortCode = 5
	}
}