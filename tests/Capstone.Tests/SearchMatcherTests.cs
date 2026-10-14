using SharpQuest.Contracts;
namespace Capstone.Tests;

public class SearchMatcherTests
{
    private static readonly SearchMatcher Matcher = SearchMatcher.Create();
 
    //R1:An empty or blank query matches every non-null text
 
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    [InlineData("\t \n")]
    public void Empty_or_blank_query_matches_any_text(string query)
    {
        Assert.True(Matcher.Matches("Pikachu", query), "R1: an empty or whitespace-only query should match every non-null text.");
    }
 
    [Fact]
    public void Empty_query_matches_an_empty_text()
    {
        Assert.True(Matcher.Matches("", ""), "R1: 'every non-null text' includes the empty text.");
    }
 
    //R2: case is ignored
 
    [Theory]
    [InlineData("Pikachu", "PIKA")]
    [InlineData("PIKACHU", "pika")]
    [InlineData("pikachu", "PiKaChU")]
    public void Matching_ignores_case(string text, string query)
    {
        Assert.True(Matcher.Matches(text, query), $"R2: '{query}' should match '{text}' whatever the case.");
    }
 
    //R3: diacritics are ignored, in both directions
 
    [Theory]
    [InlineData("Caf\u00E9", "cafe")]          // accent in the text only
    [InlineData("Cafe", "caf\u00E9")]          // accent in the query only
    [InlineData("Sosea", "\u0219osea")]        // ș in the query only
    [InlineData("\u0219osea", "sosea")]        // ș in the text only
    public void Matching_ignores_diacritics_both_directions(string text, string query)
    {
        Assert.True(Matcher.Matches(text, query), $"R3: '{query}' should match '{text}' ignoring diacritics.");
    }
 
    //R4: every word of the query must occur
 
    [Theory]
    [InlineData("fire lizard", "fire lizard", true)]
    [InlineData("fire lizard", "fire dragon", false)]      // one word missing
    [InlineData("fire lizard", "dragon lizard", false)]    // the other word missing
    [InlineData("fire lizard", "  fire \t lizard  ", true)] // split on any whitespace
    public void Every_query_word_must_occur_in_the_text(string text, string query, bool expected)
    {
        Assert.Equal(expected, Matcher.Matches(text, query));
    }
 
    //R5: word order doesn't matter
 
    [Theory]
    [InlineData("fire lizard", "lizard fire")]
    [InlineData("big red dog", "dog big red")]
    public void Word_order_does_not_matter(string text, string query)
    {
        Assert.True(Matcher.Matches(text, query), $"R5: '{query}' should match '{text}' in any word order.");
    }
 
    //R6: a word may occur inside a longer word
 
    [Theory]
    [InlineData("Pikachu", "chu")]      // end of a word
    [InlineData("Pikachu", "ika")]      // middle of a word
    [InlineData("fire lizard", "zar")]  // middle of the second word
    public void A_word_can_occur_inside_longer_word(string text, string query)
    {
        Assert.True(Matcher.Matches(text, query), $"R6: '{query}' should match inside '{text}'.");
    }
 
    //R7: a null text never matches
 
    [Theory]
    [InlineData("pika")]
    [InlineData("")]
    [InlineData("   ")]
    public void A_null_text_never_matches_and_does_not_throw(string query)
    {
        bool result = true;
 
        var ex = Record.Exception(() => result = Matcher.Matches(null, query));
 
        Assert.Null(ex);
        Assert.False(result, "R7: a null text must give false.");
    }
 
    //R8: a null query throws ArgumentNullException
 
    [Fact]
    public void A_null_query_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => { Matcher.Matches("Pikachu", null!); });
    }
}