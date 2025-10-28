namespace Letmein.Core.Configuration
{
	public enum IdGenerationType
	{
		Default = Bip39TwoWords,

		RandomWithPronounceable = 1,
		Pronounceable = 2,
		ShortPronounceable = 3,
		ShortMixedCase = 4,
		ShortCode = 5,
		Bip39TwoWords = 6,
		Bip39TwoWordsAndNumber = 7,
    }
}