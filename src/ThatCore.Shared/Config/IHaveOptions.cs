namespace ThatCore.Config;

public interface IHaveOptions<TTarget, TSource>
{
    IOptionBuilder<TTarget, TSource> AddOption();
}


