using Bingyan;
public static class AudioMap
{
	public static class BGM
	{
		public static readonly AudioRef Title = new AudioRef("BGM/Title");
		public static readonly AudioRef Battle = new AudioRef("BGM/Battle");
	}
	public static class UI
	{
		public static readonly AudioRef Point = new AudioRef("UI/Point");
		public static readonly AudioRef Click = new AudioRef("UI/Click");
		public static readonly AudioRef Popup = new AudioRef("UI/Popup");
	}
	public static class Sword
	{
		public static readonly AudioRef Use = new AudioRef("Sword/Use");
		public static readonly AudioRef Hit = new AudioRef("Sword/Hit");
	}
	public static class Lance
	{
		public static readonly AudioRef Use = new AudioRef("Lance/Use");
		public static readonly AudioRef Hit = new AudioRef("Lance/Hit");
	}
	public static class Cat
	{
		public static readonly AudioRef Walk = new AudioRef("Cat/Walk");
		public static readonly AudioRef Die = new AudioRef("Cat/Die");
		public static readonly AudioRef Run = new AudioRef("Cat/Run");
	}
	public static class Bull
	{
		public static readonly AudioRef Roar = new AudioRef("Bull/Roar");
	}
}