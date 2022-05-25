namespace toodue;

public class Scheduler
{
	
}

/* This class will probably not be implemented for a long time.
That being said, here's the goals.

# Repeat every monday
monday

# Repeat every monday and tuesday
monday&tuesday

# Next monday and forget
ONCE monday

# Repeat every monday at 8:30 and tuesday at 13:45
monday@8:30&tuesday@13:45

# Repeat every first monday of a month
monday%0

# Repeat every last monday of a month
monday%-1

# Repeat every 5th of a month
$5

# Repeat every 5th last day of a month
$-5

# Repeat alarm every 5th of a month
ALARM $5@7:00

# Repeat every day
every

# Repeat every other day from date
every^2F2022/01/20

# Repeat every three day from date
every^3F2022/01/20

# Repeat every other week on monday from date
monday^2F2022/01/20

# Repeat every other month on first of monday
monday%0^2

# Every other monday, if the monday is the first monday of the month
monday^2%0

*/