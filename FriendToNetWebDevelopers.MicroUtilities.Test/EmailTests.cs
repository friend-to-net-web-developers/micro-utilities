namespace FriendToNetWebDevelopers.MicroUtilities.Test;

/// <summary>
/// Testing base on external gist
/// </summary>
/// <seealso href="https://gist.github.com/cjaoude/fd9910626629b53c4d25"/>
[TestFixture]
public class EmailTests
{
    [Test]
    public void Email_Valid_Standard()
    {
        Assert.That(Utilities.Email.IsValidEmail("email@example.com"));
        Assert.That(Utilities.Email.IsValidEmail("firstname.lastname@example.com"));
        Assert.That(Utilities.Email.IsValidEmail("email@subdomain.example.com"));
        Assert.That(Utilities.Email.IsValidEmail("firstname+lastname@example.com"));
        Assert.That(Utilities.Email.IsValidEmail("email@123.123.123.123"));
        Assert.That(Utilities.Email.IsValidEmail("1234567890@example.com"));
        Assert.That(Utilities.Email.IsValidEmail("_______@example.com"));
        Assert.That(Utilities.Email.IsValidEmail("email@example.name"));
        Assert.That(Utilities.Email.IsValidEmail("email@example.museum"));
        Assert.That(Utilities.Email.IsValidEmail("email@example.co.jp"));
        Assert.That(Utilities.Email.IsValidEmail("firstname-lastname@example.com"));
    }

    [Test]
    public void Email_Invalid_Strange()
    {
        Assert.That(Utilities.Email.IsValidEmail("”(),:;<>[\\]@example.com"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail("just”not”right@example.com"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail("this\\ is\"really\"not\\allowed@example.com"), Is.False);
    }

    [Test]
    public void Email_Invalid_Standard()
    {
        Assert.That(Utilities.Email.IsValidEmail("plainaddress"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail("#@%^%#$@#$@#.com"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail("@example.com"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail("Joe Smith <email@example.com>"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail("email.example.com"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail("email@example@example.com"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail(".email@example.com"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail("email.@example.com"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail("email..email@example.com"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail("email@example.com (Joe Smith)"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail("email@example"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail("email@-example.com"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail("email@example.web"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail("email@111.222.333.44444"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail("email@example..com"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail("Abc..123@example.com"), Is.False);
        Assert.That(Utilities.Email.IsValidEmail("あいうえお@example.com"), Is.False);
    }
}