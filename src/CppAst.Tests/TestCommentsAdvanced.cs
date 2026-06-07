// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Linq;

namespace CppAst.Tests;

public class TestCommentsAdvanced : InlineTestBase
{
    [Test]
    public void TestBlockDoxygenTrailingAndParameterComments()
    {
        ParseAssert(@"
/// Namespace documentation.
namespace docs
{
/** Typedef documentation. */
typedef int DocumentedInt;

/** Struct documentation. */
struct Documented
{
    /** Field documentation. */
    int field;
};

/** Enum documentation. */
enum DocumentedEnum
{
    /** Item A documentation. */
    ItemA = 0,
    ///< Item B trailing documentation.
    ItemB = 1,
};

/// Trailing field declaration.
int trailingField; ///< trailing field detail

/**
 * @brief Adds two values.
 * @param lhs left value
 * @param rhs right value
 * @return sum value
 */
int add(int lhs, int rhs);
}
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);

                var docs = compilation.Namespaces.Single(x => x.Name == "docs");
                Assert.NotNull(docs.Comment);
                Assert.True(docs.Comment.ToString().Contains("Namespace documentation"));

                var typedef = docs.Typedefs.Single(x => x.Name == "DocumentedInt");
                Assert.NotNull(typedef.Comment);
                Assert.True(typedef.Comment.ToString().Contains("Typedef documentation"));

                var documented = docs.Classes.Single(x => x.Name == "Documented");
                Assert.NotNull(documented.Comment);
                Assert.True(documented.Comment.ToString().Contains("Struct documentation"));
                Assert.NotNull(documented.Fields[0].Comment);
                Assert.True(documented.Fields[0].Comment.ToString().Contains("Field documentation"));

                var documentedEnum = docs.Enums.Single(x => x.Name == "DocumentedEnum");
                Assert.NotNull(documentedEnum.Comment);
                Assert.True(documentedEnum.Comment.ToString().Contains("Enum documentation"));
                Assert.True(documentedEnum.Items[0].Comment.ToString().Contains("Item A documentation"));
                if (documentedEnum.Items[1].Comment is not null)
                {
                    Assert.True(documentedEnum.Items[1].Comment.ToString().Contains("Item B trailing documentation"));
                }

                var trailingField = docs.Fields.Single(x => x.Name == "trailingField");
                Assert.NotNull(trailingField.Comment);
                Assert.True(trailingField.Comment.ToString().Contains("Trailing field declaration") || trailingField.Comment.ToString().Contains("trailing field detail"));

                var add = docs.Functions.Single(x => x.Name == "add");
                Assert.NotNull(add.Comment);
                Assert.True(add.Comment.ToString().Contains("Adds two values"));
                Assert.True(add.Comment.ToString().Contains("@return sum value"));

                Assert.IsInstanceOf<CppCommentParamCommand>(add.Parameters[0].Comment);
                var lhsComment = (CppCommentParamCommand)add.Parameters[0].Comment;
                Assert.AreEqual("lhs", lhsComment.ParamName);
                Assert.True(lhsComment.ToString().Contains("left value"));

                Assert.IsInstanceOf<CppCommentParamCommand>(add.Parameters[1].Comment);
                var rhsComment = (CppCommentParamCommand)add.Parameters[1].Comment;
                Assert.AreEqual("rhs", rhsComment.ParamName);
                Assert.True(rhsComment.ToString().Contains("right value"));
            });
    }
}
