import { DataTypes, Model, Sequelize } from "@sequelize/core";
export const db = new Sequelize('sqlite://database.sqlite', {logging: false});

export class User extends Model {
}

User.init({
    username: {
        type: DataTypes.STRING,
        allowNull: false,
        unique: true,
        primaryKey: true
    },
    password: {
        type: DataTypes.STRING
    },
    uptime_seconds: DataTypes.INTEGER,
    ip: DataTypes.STRING
}, {sequelize: db, modelName: 'user'});
