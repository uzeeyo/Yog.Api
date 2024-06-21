using Supabase;
using Yog.Database;

public interface ISupabaseClient
{
    public Client Connection { get; }
}