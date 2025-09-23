FNA example doesn't run out-of-box, since FNA doesn't provide NuGet. You have to setup it first.

**Note:** I had to add NPE hack in the beginning of the **SDL3_FNAPlatform.PollEvents** method

```cs
if(textInputControlDown == null)
{
    textInputControlDown = new bool[7];
}
```

There could be better solution.
