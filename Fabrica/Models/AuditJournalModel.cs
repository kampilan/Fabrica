
// ReSharper disable UnusedMember.Global

/*
The MIT License (MIT)

Copyright (c) 2021 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Fabrica.Models.Serialization;
using Fabrica.Models.Support;
using Fabrica.Utilities.Text;


namespace Fabrica.Models
{


    /// <summary>
    /// Audit Journal Entity. Used by the auditing system to persist audit journals to
    /// the database
    /// </summary>
    public class AuditJournalModel: BaseMutableModel<AuditJournalModel>, IRootModel, IExplorableModel
    {


        [ModelMeta(Scope = PropertyScope.Exclude)]
        public override long Id { get; protected set; }

        [ModelMeta(Scope = PropertyScope.Immutable)]
        public override string Uid { get; set; } = Base62Converter.NewGuid();

        [ModelMeta(Scope = PropertyScope.Immutable)]
        public virtual string UnitOfWorkUid { get; set; } = "";

        [ModelMeta(Scope = PropertyScope.Immutable)]
        public virtual string SubjectUid { get; set; } = "";

        [ModelMeta(Scope = PropertyScope.Immutable)]
        public virtual string SubjectDescription { get; set; } = "";

        [ModelMeta(Scope = PropertyScope.Immutable)]
        public virtual DateTime Occurred { get; set; } = new (1883, 11, 19, 0, 0, 0, 0);

        [ModelMeta(Scope = PropertyScope.Immutable)]
        public virtual string TypeCode { get; set; } = "";

        [ModelMeta(Scope = PropertyScope.Immutable)]
        public virtual string Entity { get; set; } = "";

        [ModelMeta(Scope = PropertyScope.Immutable)]
        public virtual string EntityUid { get; set; } = "";

        [ModelMeta(Scope = PropertyScope.Immutable)]
        public virtual string EntityDescription { get; set; } = "";


        [ModelMeta(Scope = PropertyScope.Immutable)]
        public virtual string PropertyName { get; set; } = "";

        [ModelMeta(Scope = PropertyScope.Immutable)]
        public virtual string PreviousValue { get; set; } = "";

        [ModelMeta(Scope = PropertyScope.Immutable)]
        public virtual string CurrentValue { get; set; } = "";


    }

}
