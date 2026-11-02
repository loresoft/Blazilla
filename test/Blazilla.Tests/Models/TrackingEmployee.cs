using BlazorShared.Models;

namespace Blazilla.Tests.Models;

public record TrackingEmployee :
    Employee
{
    public new Address? HomeAddress
    {
        get
        {
            if (TrackingEnabled)
            {
                HomeAddressReadCount++;
            }

            return base.HomeAddress;
        }

        set => base.HomeAddress = value;
    }

    public int HomeAddressReadCount { get; private set; }

    public bool TrackingEnabled { get; set; }

    #region HashCode and Equality
    // Explicitly implemented to exclude tracking properties, which could change the hash after the instance is in the HashSet.

    public override int GetHashCode() =>
        base.GetHashCode();

    public virtual bool Equals(
        TrackingEmployee? other) =>
        (object)this == other || base.Equals(other);
    #endregion
}
