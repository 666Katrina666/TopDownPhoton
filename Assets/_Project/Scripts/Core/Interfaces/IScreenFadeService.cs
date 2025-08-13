using System.Threading.Tasks;

/// <summary>
/// Интерфейс экранного затемнения/проявления поверх сцен
/// </summary>
public interface IScreenFadeService
{
	/// <summary>
	/// Плавное затемнение экрана до чёрного
	/// </summary>
	Task FadeOut(float duration = 0.25f);

	/// <summary>
	/// Плавное проявление экрана из чёрного
	/// </summary>
	Task FadeIn(float duration = 0.25f);

	/// <summary>
	/// Мгновенно устанавливает альфу затемнения
	/// </summary>
	void SetInstant(float alpha);
}


