using System.Diagnostics;
using Duck.Renderer.Device;
using Duck.Ui.Elements;

namespace Duck.Ui;

public class Context
{
    #region Properties

    public Span<Root> Roots => GetFactory<RootFactory, Root>().Roots;



    private readonly RenderList _renderList = new();
    
    #endregion

    #region Members

    private readonly Dictionary<Type, IElementFactory> _factories = new();
    private IElementFactory[]? fff;

    #endregion

    public void AddElementType<T>(IElementFactory elementFactory)
        where T : struct
    {
        _factories.Add(typeof(T), elementFactory);
    }

    public FactoryType GetFactory<FactoryType, ElementType>()
        where FactoryType : class, IElementFactory
        where ElementType : struct
    {
        var factory = _factories[typeof(ElementType)] as FactoryType;
        Debug.Assert(factory != null);
        return factory;
    }

    public void BeginFrame()
    {
        if (null == fff) {
            fff = _factories.Values.ToArray();
        }

        for (var i = 0; i < fff.Length; i++) {
            fff[i].BeginFrame();
        }
    }

    public void New(in Fragment? child0 = null, in Fragment? child1 = null, in Fragment? child2 = null, in Fragment? child3 = null, in Fragment? child4 = null, in Fragment? child5 = null)
    {
        New(RootProps.Default, child0, child1, child2, child3, child4, child5);
    }

    public void New(in RootProps props, in Fragment? child0 = null, in Fragment? child1 = null, in Fragment? child2 = null, in Fragment? child3 = null, in Fragment? child4 = null, in Fragment? child5 = null)
    {
        GetFactory<RootFactory, Root>().Create(props, child0, child1, child2, child3, child4, child5);
    }
}
