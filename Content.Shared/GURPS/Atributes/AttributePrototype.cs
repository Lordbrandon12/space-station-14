using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Robust.Shared.Prototypes;

namespace Content.Shared.GURPS.Atributes
{
    [Prototype]
    public sealed class AttributePrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField("name")]
        public string Name { get; private set; } = string.Empty;

        [ViewVariables(VVAccess.ReadOnly)]
        public string LocalizedName => Loc.GetString(Name);

        [DataField("abreviation")]
        public string Abreviation { get; private set; } = string.Empty;

        [ViewVariables(VVAccess.ReadOnly)]
        [DataField("baseValue")]
        public int BaseValue { get; private set; } = 10;
    }
}
