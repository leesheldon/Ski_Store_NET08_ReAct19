import { DarkMode, LightMode } from "@mui/icons-material";
import { AppBar, IconButton, Toolbar, Typography } from "@mui/material";

type Props = {
    toggleDarkMode: () => void;
    darkMode: boolean;
}

export default function NavBar({darkMode, toggleDarkMode}: Props) {
    
    return (
        <AppBar position="fixed">
            <Toolbar>
                <Typography variant="h6">SKI-STORE</Typography>
                <IconButton onClick={toggleDarkMode}>
                    {darkMode ? <LightMode sx={{color: 'yellow'}} /> : <DarkMode /> }
                </IconButton>
            </Toolbar>
        </AppBar>
    );
}
